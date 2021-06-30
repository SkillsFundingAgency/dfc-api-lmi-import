using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.GraphData;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IGraphService
    {
        Task<bool> ImportAsync(GraphSocDatasetModel? graphSocDataset, GraphReplicaSet graphReplicaSet);

        Task PublishAsync(GraphReplicaSet fromGraphReplicaSet, GraphReplicaSet toGraphReplicaSet);

        Task PurgeAsync(GraphReplicaSet graphReplicaSet);

        Task PurgeSocAsync(int soc, GraphReplicaSet graphReplicaSet);
    }
}
