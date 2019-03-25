using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace AzureBatchDemoApp
{
    public class MongoConnectionFactory : IMongoConnectionFactory
    {
        private readonly IMongoClient MongoClient;
        private readonly MongoConnection MongoConnection;
        public MongoConnectionFactory(IConfiguration configuration)
        {
            this.MongoConnection = configuration.GetSection("MongoConnection").Get<MongoConnection>();
            this.MongoClient = new MongoClient(MongoConnection.ConnectionString);
        }

        public IMongoDatabase GetDatabase()
        {
            return this.MongoClient.GetDatabase(this.MongoConnection.DbName);
        }
    }
}
