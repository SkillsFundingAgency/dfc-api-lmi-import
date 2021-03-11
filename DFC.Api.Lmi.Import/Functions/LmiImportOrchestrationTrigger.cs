using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.FunctionRequestModels;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Functions
{
    public class LmiImportOrchestrationTrigger
    {
        private readonly ILogger<LmiImportOrchestrationTrigger> logger;
        private readonly IJobProfileService jobProfileService;
        private readonly IMapLmiToGraphService mapLmiToGraphService;
        private readonly ILmiSocImportService lmiSocImportService;
        private readonly IGraphService graphService;
        private readonly IEventGridService eventGridService;
        private readonly EventGridClientOptions eventGridClientOptions;

        public LmiImportOrchestrationTrigger(
            ILogger<LmiImportOrchestrationTrigger> logger,
            IJobProfileService jobProfileService,
            IMapLmiToGraphService mapLmiToGraphService,
            ILmiSocImportService lmiSocImportService,
            IGraphService graphService,
            IEventGridService eventGridService,
            EventGridClientOptions eventGridClientOptions)
        {
            this.logger = logger;
            this.jobProfileService = jobProfileService;
            this.mapLmiToGraphService = mapLmiToGraphService;
            this.lmiSocImportService = lmiSocImportService;
            this.graphService = graphService;
            this.eventGridService = eventGridService;
            this.eventGridClientOptions = eventGridClientOptions;
        }

        [FunctionName(nameof(GraphRefreshSocOrchestrator))]
        public async Task<bool> GraphRefreshSocOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var socRequest = context.GetInput<SocRequestModel>();
            var socJobProfileMapping = new SocJobProfileMappingModel { Soc = socRequest.Soc };

            await context.CallActivityAsync(nameof(GraphPurgeSocActivity), socRequest.Soc).ConfigureAwait(true);

            return await context.CallActivityAsync<bool>(nameof(ImportSocItemActivity), socJobProfileMapping).ConfigureAwait(true);
        }

        [FunctionName(nameof(GraphPurgeSocOrchestrator))]
        public async Task GraphPurgeSocOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var socRequest = context.GetInput<SocRequestModel>();

            await context.CallActivityAsync(nameof(GraphPurgeSocActivity), socRequest.Soc).ConfigureAwait(true);
        }

        [FunctionName(nameof(GraphPurgeOrchestrator))]
        public async Task GraphPurgeOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync(nameof(GraphPurgeActivity), null).ConfigureAwait(true);
        }

        [FunctionName(nameof(GraphRefreshOrchestrator))]
        [Timeout("01:00:00")]
        public async Task GraphRefreshOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            logger.LogInformation("Start importing of LMI data from API");

            var socJobProfileMappings = await context.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(GetJobProfileSocMappingsActivity), null).ConfigureAwait(true);

            if (socJobProfileMappings != null && socJobProfileMappings.Any())
            {
                await context.CallActivityAsync(nameof(GraphPurgeActivity), null).ConfigureAwait(true);

                logger.LogInformation($"Importing {socJobProfileMappings.Count} SOC mappings");

                var parallelTasks = new List<Task<bool>>();

                foreach (var socJobProfileMapping in socJobProfileMappings)
                {
                    parallelTasks.Add(context.CallActivityAsync<bool>(nameof(ImportSocItemActivity), socJobProfileMapping));
                }

                await Task.WhenAll(parallelTasks).ConfigureAwait(true);

                await context.CallActivityAsync(nameof(PostGraphSocRefreshedActivity), "LMI Import refreshed").ConfigureAwait(true);

                int importedToGraphCount = parallelTasks.Count(t => t.Result);

                logger.LogInformation($"Imported to Graph {importedToGraphCount} of {socJobProfileMappings.Count} SOC mappings");
            }
            else
            {
                logger.LogWarning("No data available from JOB profile SOC mappings - no data imported");
            }
        }

        [FunctionName(nameof(GetJobProfileSocMappingsActivity))]
        public async Task<IList<SocJobProfileMappingModel>?> GetJobProfileSocMappingsActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Getting Job profile to SOC mappings");

            return await jobProfileService.GetMappingsAsync().ConfigureAwait(false);
        }

        [FunctionName(nameof(GraphPurgeActivity))]
        public async Task GraphPurgeActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Purging Graph of all SOC");

            await graphService.PurgeAsync().ConfigureAwait(false);
        }

        [FunctionName(nameof(GraphPurgeSocActivity))]
        public async Task GraphPurgeSocActivity([ActivityTrigger] int soc)
        {
            logger.LogInformation($"Purging Graph of SOC: {soc}");

            await graphService.PurgeSocAsync(soc).ConfigureAwait(false);
        }

        [FunctionName(nameof(ImportSocItemActivity))]
        public async Task<bool> ImportSocItemActivity([ActivityTrigger] SocJobProfileMappingModel socJobProfileMapping)
        {
            logger.LogInformation($"Importing SOC: {socJobProfileMapping.Soc}");

            var lmiSocDataset = await lmiSocImportService.ImportAsync(socJobProfileMapping.Soc!.Value, socJobProfileMapping.JobProfiles).ConfigureAwait(false);
            if (lmiSocDataset != null)
            {
                var graphSocDataset = mapLmiToGraphService.Map(lmiSocDataset);

                if (await graphService.ImportAsync(graphSocDataset).ConfigureAwait(false))
                {
                    return true;
                }
            }

            return false;
        }

        [FunctionName(nameof(PostGraphSocRefreshedActivity))]
        public async Task PostGraphSocRefreshedActivity([ActivityTrigger] string displayText)
        {
            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = $"{eventGridClientOptions.ApiEndpoint}",
                DisplayText = displayText,
                VersionId = Guid.NewGuid().ToString(),
                Author = eventGridClientOptions.SubjectPrefix,
            };

            await eventGridService.SendEventAsync(WebhookCacheOperation.CreateOrUpdate, eventGridEventData, eventGridClientOptions.SubjectPrefix).ConfigureAwait(false);
        }
    }
}