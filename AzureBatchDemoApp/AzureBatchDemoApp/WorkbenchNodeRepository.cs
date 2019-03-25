using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AzureBatchDemoApp
{
    public class WorkbenchNodeRepository : IWorkbenchRepository
    {
        private readonly IMongoCollection<WorkbenchNode> MongoCollection;
        private const string collectionName = "workbenchNode";
        public WorkbenchNodeRepository(IMongoCollectionFactory<WorkbenchNode> nodeCollectionFactory)
        {
            MongoCollection = nodeCollectionFactory.GetMongoCollection(collectionName);
        }

        public async Task<List<WorkbenchNode>> GetWorkbenchNodes()
        {
            return await this.MongoCollection.AsQueryable().ToListAsync();
        }

        public async Task UpdateWorkbenchNode(WorkbenchNode node)
        {
            await MongoCollection.FindOneAndReplaceAsync(
                Builders<WorkbenchNode>.Filter.Where(x => x.NodeId == node.NodeId && x.IsFolder == node.IsFolder),node);
        }
    }
}