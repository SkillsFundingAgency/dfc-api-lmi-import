using DFC.Api.Lmi.Import.Models.JobProfileApi;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using System.Collections.Generic;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IJobProfilesToSocMappingService
    {
        IList<SocJobProfileMappingModel> Map(IList<JobProfileDetailModel> jobProfileDetails);
    }
}
