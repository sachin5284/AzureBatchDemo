using MongoDB.Driver;

namespace AzureBatchDemoApp
{
    public interface IMongoCollectionFactory<T>
    {
        IMongoCollection<T> GetMongoCollection(string documentName);
    }
}