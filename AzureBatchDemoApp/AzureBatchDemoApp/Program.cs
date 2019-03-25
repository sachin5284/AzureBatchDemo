using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AzureBatchDemoApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new Startup().GetServiceProvider();
            
            IWorkbenchRepository workbenchRepository = serviceProvider.GetService<IWorkbenchRepository>();

            var workbenchNode = (await workbenchRepository.GetWorkbenchNodes()).Single(node => node.NodeId == 1);
            workbenchNode.ParentFolderId = 1;
            await workbenchRepository.UpdateWorkbenchNode(workbenchNode);
            Console.WriteLine("Task Complete!!");
            await Task.Delay(2000);
        }
    }
}