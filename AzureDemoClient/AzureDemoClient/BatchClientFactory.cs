using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Extensions.Configuration;

namespace AzureDemoClient
{
    public class BatchClientFactory : IBatchClientFactory
    {
        private readonly ApplicationInformation AppInformation;
        private readonly PoolKeys PoolKeys;
        private readonly BatchShareKey ShareKey;

        public BatchClientFactory(IConfiguration configuration)
        {
            ShareKey = configuration.GetSection("BatchShareKey").Get<BatchShareKey>();
            PoolKeys = configuration.GetSection("PoolKeys").Get<PoolKeys>();
            AppInformation = configuration.GetSection("ApplicationInformation").Get<ApplicationInformation>();
        }

        public async Task<BatchClient> GetBatchClient()
        {
            BatchClient batchClient = BatchClient.Open(new BatchSharedKeyCredentials(ShareKey.BatchAccountUrl,
                ShareKey.BatchAccountName, ShareKey.BatchAccountKey));

            await this.CreatePoolIfNotExistAsync(batchClient);
            await this.CreateJobTask(batchClient);
            await this.AddTaskAsync(batchClient);
            return batchClient;

        }

        private async Task CreatePoolIfNotExistAsync(BatchClient batchClient)
        {
            try
            {
                Console.WriteLine("Creating pool [{0}]...", PoolKeys.PoolId);

                var imageReference = new ImageReference(
                            publisher: "MicrosoftWindowsServer",
                            offer: "WindowsServer",
                            sku: "2012-R2-Datacenter-smalldisk",
                            version: "latest");

                var virtualMachineConfiguration =
                    new VirtualMachineConfiguration(
                        imageReference,
                        "batch.node.windows amd64");

                // Create an unbound pool. No pool is actually created in the Batch service until we call
                // CloudPool.Commit(). This CloudPool instance is therefore considered "unbound," and we can
                // modify its properties.
                var pool = batchClient.PoolOperations.CreatePool(
                        PoolKeys.PoolId,
                        targetDedicatedComputeNodes: PoolKeys.DedicatedNodeCount,
                        targetLowPriorityComputeNodes: PoolKeys.LowPriorityNodeCount,
                        virtualMachineSize: PoolKeys.PoolVMSize,
                        virtualMachineConfiguration: virtualMachineConfiguration);

                pool.ApplicationPackageReferences = new List<ApplicationPackageReference>
                {
                    new ApplicationPackageReference
                    {
                        ApplicationId = AppInformation.PackageId,
                        Version = AppInformation.Version
                    }
                };

                await pool.CommitAsync();
            }
            catch (BatchException be)
            {
                // Accept the specific error code PoolExists as that is expected if the pool already exists
                if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                    Console.WriteLine("The pool {0} already existed when we tried to create it", PoolKeys.PoolId);
                else
                    throw; // Any other exception is unexpected
            }
        }

        private async Task CreateJobTask(BatchClient batchClient)
        {
            Console.WriteLine("Creating job [{0}]...", PoolKeys.JobId);

            CloudJob job = batchClient.JobOperations.CreateJob();
            job.Id = PoolKeys.JobId;
            job.Priority = 1000;
            job.PoolInformation = new PoolInformation { PoolId = PoolKeys.PoolId };

            await job.CommitAsync();
        }

        private async Task<List<CloudTask>> AddTaskAsync(BatchClient batchClient)
        {
            Console.WriteLine("Adding tasks to job [{0}]...", PoolKeys.JobId);

            string appPath = $"%AZ_BATCH_APP_PACKAGE_{AppInformation.PackageId}#{AppInformation.Version}%";

            string taskCommandLine = $"cmd /c {appPath}\\publish\\AzureBatchDemoApp.exe";

            // Create a collection to hold the tasks added to the job:
            List<CloudTask> tasks = new List<CloudTask>
            {
                new CloudTask("Task0", taskCommandLine)
            };

            // Call BatchClient.JobOperations.AddTask() to add the tasks as a collection rather than making a
            // separate call for each. Bulk task submission helps to ensure efficient underlying API
            // calls to the Batch service. 
            await batchClient.JobOperations.AddTaskAsync(PoolKeys.JobId, tasks);
            return tasks;
        }
    }
}