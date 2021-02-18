using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.JobProfileApi;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace DFC.Api.Lmi.Import.Services
{
    public class JobProfilesToSocMappingService : IJobProfilesToSocMappingService
    {
        private readonly ILogger<JobProfilesToSocMappingService> logger;
        private readonly IJobProfileApiConnector jobProfileApiConnector;

        public JobProfilesToSocMappingService(
             ILogger<JobProfilesToSocMappingService> logger,
             IJobProfileApiConnector jobProfileApiConnector)
        {
            this.logger = logger;
            this.jobProfileApiConnector = jobProfileApiConnector;
        }

        public IList<SocJobProfileMappingModel> Map(IList<JobProfileDetailModel> jobProfileDetails)
        {
            var mappings = (from a in jobProfileDetails select a.Soc)
              .OrderBy(o => o).Distinct()
              .Select(s => new SocJobProfileMappingModel
              {
                  Soc = s,
                  JobProfiles = (from jp in jobProfileDetails
                                 where jp.Soc == s
                                 select new SocJobProfileItemModel
                                 {
                                     CanonicalName = jp.CanonicalName,
                                     Title = jp.Title,
                                 }).OrderBy(o => o.CanonicalName).ToList(),
              })
              .ToList();

            return mappings;
        }
    }
}
