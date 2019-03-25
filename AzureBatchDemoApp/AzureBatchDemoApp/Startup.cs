using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureBatchDemoApp
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
            ServiceCollection.AddSingleton<IMongoConnectionFactory, MongoConnectionFactory>();
            ServiceCollection
                .AddScoped<IMongoCollectionFactory<WorkbenchNode>, MongoCollectionFactory<WorkbenchNode>>();
            ServiceCollection.AddSingleton<IWorkbenchRepository, WorkbenchNodeRepository>();
            ServiceCollection.AddSingleton(Configuration);
            return this.ServiceCollection.BuildServiceProvider();
        }
    }
}