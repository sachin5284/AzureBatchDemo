namespace AzureDemoClient
{
    public class BatchShareKey
    {
        public string BatchAccountUrl { get; set; }
        public string BatchAccountName { get; set; }
        public string BatchAccountKey { get; set; }
    }

    public class PoolKeys
    {
        public string PoolId { get; set; }
        public int DedicatedNodeCount { get; set; }
        public int LowPriorityNodeCount { get; set; }
        public string PoolVMSize { get; set; }
        public string JobId { get; set; }

    }

    public class ApplicationInformation
    {
        public string PackageId { get; set; }
        public string Version { get; set; }
    }

    public class ContainerName
    {
        public string Input { get; set; }
        public string Output { get; set; }
    }

    public class StorageAccountKeys
    {
        public string Name { get; set; }
        public string Key { get; set; }
    }
}