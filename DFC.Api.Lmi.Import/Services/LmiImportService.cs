using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    public class LmiImportService : ILmiImportService
    {
        private readonly ILogger<LmiImportService> logger;
        private readonly IMapLmiToGraphService mapLmiToGraphService;
        private readonly IJobProfileService jobProfileService;
        private readonly ILmiSocImportService lmiSocImportService;
        private readonly IGraphService graphService;
        private readonly IEventGridService eventGridService;
        private readonly EventGridClientOptions eventGridClientOptions;

        public LmiImportService(
            ILogger<LmiImportService> logger,
            IMapLmiToGraphService mapLmiToGraphService,
            IJobProfileService jobProfileService,
            ILmiSocImportService lmiSocImportService,
            IGraphService graphService,
            IEventGridService eventGridService,
            EventGridClientOptions eventGridClientOptions)
        {
            this.logger = logger;
            this.mapLmiToGraphService = mapLmiToGraphService;
            this.jobProfileService = jobProfileService;
            this.lmiSocImportService = lmiSocImportService;
            this.graphService = graphService;
            this.eventGridService = eventGridService;
            this.eventGridClientOptions = eventGridClientOptions;
        }

        public async Task ImportAsync()
        {
            logger.LogInformation("Start importing of LMI data from API");

            var socJobProfileMappings = await jobProfileService.GetMappingsAsync().ConfigureAwait(false);

            if (socJobProfileMappings != null && socJobProfileMappings.Any())
            {
                await graphService.PurgeAsync().ConfigureAwait(false);

                int importedToGraphCount = 0;

                logger.LogInformation($"Importing {socJobProfileMappings.Count} SOC mappings");

                foreach (var socJobProfileMapping in socJobProfileMappings)
                {
                    if (await ImportItemAsync(socJobProfileMapping.Soc!.Value, socJobProfileMapping.JobProfiles).ConfigureAwait(false))
                    {
                        importedToGraphCount++;
                    }
                }

                var eventGridEventData = new EventGridEventData
                {
                    ItemId = Guid.NewGuid().ToString(),
                    Api = $"{eventGridClientOptions.ApiEndpoint}",
                    DisplayText = "LMI Import refreshed",
                    VersionId = Guid.NewGuid().ToString(),
                    Author = eventGridClientOptions.SubjectPrefix,
                };

                await eventGridService.SendEventAsync(WebhookCacheOperation.CreateOrUpdate, eventGridEventData, eventGridClientOptions.SubjectPrefix).ConfigureAwait(false);

                logger.LogInformation($"Imported to Graph {importedToGraphCount} of {socJobProfileMappings.Count} SOC mappings");
            }
        }

        public async Task<bool> ImportItemAsync(int soc, List<SocJobProfileItemModel>? jobProfiles)
        {
            var lmiSocDataset = await lmiSocImportService.ImportAsync(soc, jobProfiles).ConfigureAwait(false);
            if (lmiSocDataset != null)
            {
                var graphSocDatasetModel = mapLmiToGraphService.Map(lmiSocDataset);

                if (await graphService.ImportAsync(graphSocDatasetModel).ConfigureAwait(false))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
