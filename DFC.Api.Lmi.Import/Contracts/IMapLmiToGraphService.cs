using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.Api.Lmi.Import.Models.LmiApiData;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IMapLmiToGraphService
    {
        GraphSocDatasetModel? Map(LmiSocDatasetModel lmiSocDataset);
    }
}
