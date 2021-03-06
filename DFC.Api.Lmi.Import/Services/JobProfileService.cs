﻿using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    public class JobProfileService : IJobProfileService
    {
        private readonly ILogger<JobProfileService> logger;
        private readonly IJobProfileApiConnector jobProfileApiConnector;
        private readonly IJobProfilesToSocMappingService jobProfilesToSocMappingService;
        private readonly SocJobProfilesMappingsCachedModel socJobProfilesMappingsCachedModel;

        public JobProfileService(
             ILogger<JobProfileService> logger,
             IJobProfileApiConnector jobProfileApiConnector,
             IJobProfilesToSocMappingService jobProfilesToSocMappingService,
             SocJobProfilesMappingsCachedModel socJobProfilesMappingsCachedModel)
        {
            this.logger = logger;
            this.jobProfileApiConnector = jobProfileApiConnector;
            this.jobProfilesToSocMappingService = jobProfilesToSocMappingService;
            this.socJobProfilesMappingsCachedModel = socJobProfilesMappingsCachedModel;
        }

        public async Task<IList<SocJobProfileMappingModel>?> GetMappingsAsync()
        {
            logger.LogInformation($"Retrieving from job-profiles API");

            var jobProfileSummaries = await jobProfileApiConnector.GetSummaryAsync().ConfigureAwait(false);

            if (jobProfileSummaries != null && jobProfileSummaries.Any())
            {
                logger.LogInformation($"Retrieved {jobProfileSummaries.Count} job-profiles from job-profiles API");

                var jobProfileDetails = await jobProfileApiConnector.GetDetailsAsync(jobProfileSummaries).ConfigureAwait(false);
                socJobProfilesMappingsCachedModel.SocJobProfileMappings = jobProfilesToSocMappingService.Map(jobProfileDetails);

                logger.LogInformation($"Transformed {jobProfileSummaries.Count} job-profiles into {socJobProfilesMappingsCachedModel.SocJobProfileMappings.Count} SOC / job-profile mapping");

                return socJobProfilesMappingsCachedModel.SocJobProfileMappings;
            }
            else
            {
                logger.LogWarning($"Failed to retrieve data from job-profiles API");
            }

            return default;
        }
    }
}
