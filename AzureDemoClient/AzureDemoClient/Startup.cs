using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDemoClient
{
    public class Startup
    {
        private readonly IServiceCollection ServiceCollection;
        private readonly IConfiguration Configuration;
        public Startup()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("AppSettings.json");
            Configuration = builder.Build();
            this.ServiceCollection = new ServiceCollection();
        }

        public ServiceProvider GetServiceProvider()
        {
            ServiceCollection.AddSingleton(Configuration);
            ServiceCollection.AddScoped<IBatchClientFactory, BatchClientFactory>();
            ServiceCollection.AddScoped<IBatchProcessor, BatchProcessor>();
            return this.ServiceCollection.BuildServiceProvider();
        }
    }
}