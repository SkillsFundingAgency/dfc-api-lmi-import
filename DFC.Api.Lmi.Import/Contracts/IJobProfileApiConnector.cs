using DFC.Api.Lmi.Import.Models.JobProfileApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IJobProfileApiConnector
    {
        Task<IList<JobProfileSummaryModel>?> GetSummaryAsync();

        Task<IList<JobProfileDetailModel>> GetDetailsAsync(IList<JobProfileSummaryModel>? jobProfileSummaries);
    }
}
