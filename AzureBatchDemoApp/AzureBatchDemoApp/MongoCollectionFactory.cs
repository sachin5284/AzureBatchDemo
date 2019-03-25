using MongoDB.Driver;

namespace AzureBatchDemoApp
{
    public class MongoCollectionFactory<T> : IMongoCollectionFactory<T>
    {
        private readonly IMongoDatabase MongoDatabase;

        public MongoCollectionFactory(IMongoConnectionFactory mongoConnectionFactory)
        {
            MongoDatabase = mongoConnectionFactory.GetDatabase();
        }

        public IMongoCollection<T> GetMongoCollection(string documentName)
        {
            return this.MongoDatabase.GetCollection<T>(documentName);
        }
    }
}