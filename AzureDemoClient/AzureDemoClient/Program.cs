using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDemoClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new Startup().GetServiceProvider();
            IBatchProcessor batchProcessor = serviceProvider.GetService<IBatchProcessor>();
            await batchProcessor.Process();
        }
    }
}
