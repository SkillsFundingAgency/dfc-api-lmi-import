using DFC.Api.Lmi.Import.Models.GraphData;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IGraphService
    {
        Task ImportAsync(GraphSocDatasetModel graphSocDataset);

        Task PurgeAsync();
    }
}
