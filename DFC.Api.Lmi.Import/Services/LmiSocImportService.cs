using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    public class LmiSocImportService : ILmiSocImportService
    {
        private readonly ILogger<LmiSocImportService> logger;
        private readonly ILmiApiConnector lmiApiConnector;

        public LmiSocImportService(
            ILogger<LmiSocImportService> logger,
            ILmiApiConnector lmiApiConnector)
        {
            this.logger = logger;
            this.lmiApiConnector = lmiApiConnector;
        }

        public async Task<LmiSocDatasetModel> ImportAsync(SocJobProfileMappingModel socJobProfileMappingModel)
        {
            _ = socJobProfileMappingModel?.Soc ?? throw new ArgumentNullException(nameof(socJobProfileMappingModel));

            var soc = socJobProfileMappingModel.Soc!.Value;

            logger.LogInformation($"Importing SOC '{soc}' with data from LMI API");

            var lmiSocDataset = await lmiApiConnector.ImportAsync<LmiSocDatasetModel>(soc, LmiApiClientOptions.LmiQuery.SocDetail).ConfigureAwait(false) ?? new LmiSocDatasetModel { Soc = soc };
            lmiSocDataset.JobProfiles = socJobProfileMappingModel.JobProfiles;
            lmiSocDataset.JobGrowth = await lmiApiConnector.ImportAsync<LmiPredictedModel>(soc, LmiApiClientOptions.LmiQuery.JobGrowth).ConfigureAwait(false);
            lmiSocDataset.QualificationLevel = await lmiApiConnector.ImportAsync<LmiBreakdownModel>(soc, LmiApiClientOptions.LmiQuery.QualificationLevel).ConfigureAwait(false);
            lmiSocDataset.EmploymentByRegion = await lmiApiConnector.ImportAsync<LmiBreakdownModel>(soc, LmiApiClientOptions.LmiQuery.EmploymentByRegion).ConfigureAwait(false);
            lmiSocDataset.TopIndustriesInJobGroup = await lmiApiConnector.ImportAsync<LmiBreakdownModel>(soc, LmiApiClientOptions.LmiQuery.TopIndustriesInJobGroup).ConfigureAwait(false);

            logger.LogInformation($"Imported SOC '{soc}' with data from LMI API");

            return lmiSocDataset;
        }
    }
}
