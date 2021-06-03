using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.GraphData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ISocGraphQueryService
    {
        Task<List<SocModel>> GetSummaryAsync(GraphReplicaSet graphReplicaSet);

        Task<GraphSocDatasetModel?> GetDetailAsync(GraphReplicaSet graphReplicaSet, int soc);
    }
}
