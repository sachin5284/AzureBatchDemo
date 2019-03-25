using MongoDB.Driver;

namespace AzureBatchDemoApp
{
    public interface IMongoConnectionFactory
    {
        IMongoDatabase GetDatabase();
    }
}