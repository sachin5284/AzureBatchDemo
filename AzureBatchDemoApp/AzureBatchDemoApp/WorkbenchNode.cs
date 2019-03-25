using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AzureBatchDemoApp
{
    public class WorkbenchNode
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int NodeId { get; set; }
        public int SequenceOrder { get; set; }
        public int ParentFolderId { get; set; }
        public bool IsFolder { get; set; }
        public bool IsDeleted { get; set; }
    }
}