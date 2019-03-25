using System.Threading.Tasks;

namespace AzureDemoClient
{
    public interface IBatchProcessor
    {
        Task Process();
    }
}