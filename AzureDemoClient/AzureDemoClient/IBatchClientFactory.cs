using System.Threading.Tasks;
using Microsoft.Azure.Batch;

namespace AzureDemoClient
{
    public interface IBatchClientFactory
    {
        Task<BatchClient> GetBatchClient();
    }
}