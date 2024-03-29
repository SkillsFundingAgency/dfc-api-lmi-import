﻿using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    public class LmiSocImportService : ILmiSocImportService
    {
        private readonly ILogger<LmiSocImportService> logger;
        private readonly LmiApiClientOptions lmiApiClientOptions;
        private readonly ILmiApiConnector lmiApiConnector;

        public LmiSocImportService(
            ILogger<LmiSocImportService> logger,
            LmiApiClientOptions lmiApiClientOptions,
            ILmiApiConnector lmiApiConnector)
        {
            this.logger = logger;
            this.lmiApiClientOptions = lmiApiClientOptions;
            this.lmiApiConnector = lmiApiConnector;
        }

        public async Task<LmiSocDatasetModel?> ImportAsync(int soc, List<SocJobProfileItemModel>? jobProfiles)
        {
            logger.LogInformation($"Importing SOC '{soc}' with data from LMI API");

            var lmiSocUri = lmiApiClientOptions.BuildApiUri(soc, lmiApiClientOptions.MinYear, lmiApiClientOptions.MaxYear, LmiApiQuery.SocDetail);
            var jobGrowthStartUri = lmiApiClientOptions.BuildApiUri(soc, lmiApiClientOptions.MinYear, lmiApiClientOptions.MinYear, LmiApiQuery.JobGrowth);
            var jobGrowthEndUri = lmiApiClientOptions.BuildApiUri(soc, lmiApiClientOptions.MaxYear, lmiApiClientOptions.MaxYear, LmiApiQuery.JobGrowth);
            var replacementDemandUri = lmiApiClientOptions.BuildApiUri(soc, lmiApiClientOptions.MinYear, lmiApiClientOptions.MaxYear, LmiApiQuery.ReplacementDemand);
            var qualificationLevelUri = lmiApiClientOptions.BuildApiUri(soc, lmiApiClientOptions.MinYear, lmiApiClientOptions.MinYear, LmiApiQuery.QualificationLevel);
            var employmentByRegionUri = lmiApiClientOptions.BuildApiUri(soc, lmiApiClientOptions.MinYear, lmiApiClientOptions.MinYear, LmiApiQuery.EmploymentByRegion);
            var topIndustriesInJobGroupUri = lmiApiClientOptions.BuildApiUri(soc, lmiApiClientOptions.MinYear, lmiApiClientOptions.MinYear, LmiApiQuery.TopIndustriesInJobGroup);

            var lmiSocDataset = await lmiApiConnector.ImportAsync<LmiSocDatasetModel>(lmiSocUri).ConfigureAwait(false);

            if (lmiSocDataset != null)
            {
                lmiSocDataset.JobProfiles = jobProfiles;
                lmiSocDataset.JobGrowth = await lmiApiConnector.ImportAsync<LmiPredictedModel>(jobGrowthStartUri).ConfigureAwait(false);
                lmiSocDataset.ReplacementDemand = await lmiApiConnector.ImportAsync<LmiReplacementDemandModel>(replacementDemandUri).ConfigureAwait(false);
                lmiSocDataset.QualificationLevel = await lmiApiConnector.ImportAsync<LmiBreakdownModel>(qualificationLevelUri).ConfigureAwait(false);
                lmiSocDataset.EmploymentByRegion = await lmiApiConnector.ImportAsync<LmiBreakdownModel>(employmentByRegionUri).ConfigureAwait(false);
                lmiSocDataset.TopIndustriesInJobGroup = await lmiApiConnector.ImportAsync<LmiBreakdownModel>(topIndustriesInJobGroupUri).ConfigureAwait(false);

                var jobGrowthEnd = await lmiApiConnector.ImportAsync<LmiPredictedModel>(jobGrowthEndUri).ConfigureAwait(false);

                if (lmiSocDataset.JobGrowth?.PredictedEmployment != null && jobGrowthEnd?.PredictedEmployment != null)
                {
                    lmiSocDataset.JobGrowth.PredictedEmployment.AddRange(jobGrowthEnd.PredictedEmployment);
                }

                logger.LogInformation($"Imported SOC '{soc}' with data from LMI API");

                return lmiSocDataset;
            }

            logger.LogWarning($"no data for SOC '{soc}' available from LMI API");
            return null;
        }
    }
}
