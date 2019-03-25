using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;
using Microsoft.Extensions.Configuration;
using Sandboxable.Microsoft.WindowsAzure.Storage;
using Sandboxable.Microsoft.WindowsAzure.Storage.Blob;

namespace AzureDemoClient
{
    public class BatchProcessor : IBatchProcessor
    {
        private readonly IBatchClientFactory BatchClientFactory;
        private readonly PoolKeys PoolKeys;
        private readonly ContainerName ContainerName;
        private readonly StorageAccountKeys StorageAccount;
        const string completeMessage = "All tasks reached state Completed.";
        const string incompleteMessage = "One or more tasks failed to reach the Completed state within the timeout period.";
        const string successMessage = "Success! All tasks completed successfully. Output files uploaded to output container.";
        const string failureMessage = "One or more tasks failed.";

        private static readonly TimeSpan JobTimeOut = TimeSpan.FromMinutes(30);

        public BatchProcessor(IConfiguration configuration,IBatchClientFactory batchClientFactory)
        {
            BatchClientFactory = batchClientFactory;
            PoolKeys = configuration.GetSection("PoolKeys").Get<PoolKeys>();
            ContainerName = configuration.GetSection("ContainerName").Get<ContainerName>();
            StorageAccount = configuration.GetSection("StorageAccountKeys").Get<StorageAccountKeys>();
        }

        public async Task Process()
        {
            Console.WriteLine("Sample start: {0}", DateTime.Now);
            Console.WriteLine();
            Stopwatch timer = new Stopwatch();
            timer.Start();

            var blobClient = await this.CreateBlobClient();


            using (BatchClient batchClient = await BatchClientFactory.GetBatchClient())
            {
                // Obtain the collection of tasks currently managed by the job. 
                // Use a detail level to specify that only the "id" property of each task should be populated. 
                // See https://docs.microsoft.com/en-us/azure/batch/batch-efficient-list-queries

                ODATADetailLevel detail = new ODATADetailLevel(selectClause: "id");

                List<CloudTask> addedTasks = await batchClient.JobOperations.ListTasks(PoolKeys.JobId, detail).ToListAsync();

                Console.WriteLine("Monitoring all tasks for 'Completed' state, timeout in {0}...", JobTimeOut.ToString());

                // We use a TaskStateMonitor to monitor the state of our tasks. In this case, we will wait for all tasks to
                // reach the Completed state.

                TaskStateMonitor taskStateMonitor = batchClient.Utilities.CreateTaskStateMonitor();
                try
                {
                    await taskStateMonitor.WhenAll(addedTasks, TaskState.Completed, JobTimeOut);
                }
                catch (TimeoutException)
                {
                    await batchClient.JobOperations.TerminateJobAsync(PoolKeys.JobId);
                    Console.WriteLine(incompleteMessage);
                }
                

                await batchClient.JobOperations.TerminateJobAsync(PoolKeys.JobId);
                Console.WriteLine(completeMessage);

                // All tasks have reached the "Completed" state, however, this does not guarantee all tasks completed successfully.
                // Here we further check for any tasks with an execution result of "Failure".

                // Update the detail level to populate only the executionInfo property.
                detail.SelectClause = "executionInfo";
                // Filter for tasks with 'Failure' result.
                detail.FilterClause = "executionInfo/result eq 'Failure'";

                List<CloudTask> failedTasks = await batchClient.JobOperations.ListTasks(PoolKeys.JobId, detail).ToListAsync();

                Console.WriteLine(failedTasks.Any() ? failureMessage : successMessage);

                // Delete input container in storage
                Console.WriteLine("Deleting container [{0}]...", "input");
                CloudBlobContainer container = blobClient.GetContainerReference("input");
                await container.DeleteIfExistsAsync();

                // Print out timing info
                timer.Stop();
                Console.WriteLine();
                Console.WriteLine("Sample end: {0}", DateTime.Now);
                Console.WriteLine("Elapsed time: {0}", timer.Elapsed);

                // Clean up Batch resources (if the user so chooses)
                Console.WriteLine();
                await batchClient.JobOperations.DeleteJobAsync(PoolKeys.JobId);
                Console.WriteLine("Job Done .. Press any key to exit!!");
                Console.ReadKey();
            }
        }
        private async Task<CloudBlobClient> CreateBlobClient()
        {
            // Construct the Storage account connection string
            string storageConnectionString = $"DefaultEndpointsProtocol=https;AccountName={StorageAccount.Name};AccountKey={StorageAccount.Key}";

            // Retrieve the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the blob client, for use in obtaining references to blob storage containers
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();


            // Use the blob client to create the containers in blob storage
            await this.CreateContainerIfNotExistAsync(blobClient, ContainerName.Input);
            await this.CreateContainerIfNotExistAsync(blobClient, ContainerName.Output);
            
            return blobClient;
        }
        private async Task CreateContainerIfNotExistAsync(CloudBlobClient blobClient, string containerName)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            Console.WriteLine("Creating container [{0}].", containerName);
        }

        private string GetContainerSasUrl(CloudBlobClient blobClient, string containerName, SharedAccessBlobPermissions permissions)
        {
            // Set the expiry time and permissions for the container access signature. In this case, no start time is specified,
            // so the shared access signature becomes valid immediately. Expiration is in 2 hours.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(2),
                Permissions = permissions
            };

            // Generate the shared access signature on the container, setting the constraints directly on the signature
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

            // Return the URL string for the container, including the SAS token
            return $"{container.Uri}{sasContainerToken}";
        }
    }
}