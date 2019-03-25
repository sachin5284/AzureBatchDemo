using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureBatchDemoApp
{
    public interface IWorkbenchRepository
    {
        Task<List<WorkbenchNode>> GetWorkbenchNodes();

        Task UpdateWorkbenchNode(WorkbenchNode node);
    }
}