using DFC.Api.Lmi.Import.Contracts;
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
        private const string EventTypeForDraft = "draft";
        private const string EventTypeForPublished = "published";
        private const string EventTypeForDraftDiscarded = "draft-discarded";
        private const string EventTypeForDeleted = "deleted";

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
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var socRequest = context.GetInput<SocRequestModel>();
            var socJobProfileMapping = new SocJobProfileMappingModel { Soc = socRequest.Soc };

            await context.CallActivityAsync(nameof(GraphPurgeSocActivity), socRequest.Soc).ConfigureAwait(true);

            var eventGridPostPurgeRequest = new EventGridPostRequestModel
            {
                ItemId = socRequest.SocId,
                DisplayText = $"LMI SOC purged: {socRequest.Soc}",
                EventType = socRequest.IsDraftEnvironment ? EventTypeForDraftDiscarded : EventTypeForDeleted,
            };

            await context.CallActivityAsync(nameof(PostGraphEventActivity), eventGridPostPurgeRequest).ConfigureAwait(true);

            var itemId = await context.CallActivityAsync<Guid?>(nameof(ImportSocItemActivity), socJobProfileMapping).ConfigureAwait(true);

            if (itemId != null)
            {
                var eventGridPostRequest = new EventGridPostRequestModel
                {
                    ItemId = itemId,
                    DisplayText = $"LMI SOC refreshed: {socRequest.Soc}",
                    EventType = socRequest.IsDraftEnvironment ? EventTypeForDraft : EventTypeForPublished,
                };

                await context.CallActivityAsync(nameof(PostGraphEventActivity), eventGridPostRequest).ConfigureAwait(true);

                return true;
            }

            return false;
        }

        [FunctionName(nameof(GraphPurgeSocOrchestrator))]
        public async Task GraphPurgeSocOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var socRequest = context.GetInput<SocRequestModel>();

            await context.CallActivityAsync(nameof(GraphPurgeSocActivity), socRequest.Soc).ConfigureAwait(true);

            var eventGridPostRequest = new EventGridPostRequestModel
            {
                ItemId = socRequest.SocId,
                DisplayText = $"LMI SOC purged: {socRequest.Soc}",
                EventType = socRequest.IsDraftEnvironment ? EventTypeForDraftDiscarded : EventTypeForDeleted,
            };

            await context.CallActivityAsync(nameof(PostGraphEventActivity), eventGridPostRequest).ConfigureAwait(true);
        }

        [FunctionName(nameof(GraphPurgeOrchestrator))]
        public async Task GraphPurgeOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var orchestratorRequestModel = context.GetInput<OrchestratorRequestModel>();
            await context.CallActivityAsync(nameof(GraphPurgeActivity), null).ConfigureAwait(true);

            var eventGridPostRequest = new EventGridPostRequestModel
            {
                ItemId = Guid.NewGuid(),
                DisplayText = "LMI Import purged",
                EventType = orchestratorRequestModel.IsDraftEnvironment ? EventTypeForDraftDiscarded : EventTypeForDeleted,
            };

            await context.CallActivityAsync(nameof(PostGraphEventActivity), eventGridPostRequest).ConfigureAwait(true);
        }

        [FunctionName(nameof(GraphRefreshOrchestrator))]
        [Timeout("01:00:00")]
        public async Task GraphRefreshOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            logger.LogInformation("Start importing of LMI data from API");

            var orchestratorRequestModel = context.GetInput<OrchestratorRequestModel>();
            var socJobProfileMappings = await context.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(GetJobProfileSocMappingsActivity), null).ConfigureAwait(true);

            if (socJobProfileMappings != null && socJobProfileMappings.Any())
            {
                await context.CallActivityAsync(nameof(GraphPurgeActivity), null).ConfigureAwait(true);

                logger.LogInformation($"Importing {socJobProfileMappings.Count} SOC mappings");

                var parallelTasks = new List<Task<Guid?>>();

                foreach (var socJobProfileMapping in socJobProfileMappings)
                {
                    parallelTasks.Add(context.CallActivityAsync<Guid?>(nameof(ImportSocItemActivity), socJobProfileMapping));
                }

                await Task.WhenAll(parallelTasks).ConfigureAwait(true);

                var eventGridPostRequest = new EventGridPostRequestModel
                {
                    ItemId = Guid.NewGuid(),
                    DisplayText = "LMI Import refreshed",
                    EventType = orchestratorRequestModel.IsDraftEnvironment ? EventTypeForDraft : EventTypeForPublished,
                };

                await context.CallActivityAsync(nameof(PostGraphEventActivity), eventGridPostRequest).ConfigureAwait(true);

                int importedToGraphCount = parallelTasks.Count(t => t.Result != null);

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
        public async Task<Guid?> ImportSocItemActivity([ActivityTrigger] SocJobProfileMappingModel socJobProfileMapping)
        {
            _ = socJobProfileMapping ?? throw new ArgumentNullException(nameof(socJobProfileMapping));

            logger.LogInformation($"Importing SOC: {socJobProfileMapping.Soc}");

            var lmiSocDataset = await lmiSocImportService.ImportAsync(socJobProfileMapping.Soc!.Value, socJobProfileMapping.JobProfiles).ConfigureAwait(false);
            if (lmiSocDataset != null)
            {
                var graphSocDataset = mapLmiToGraphService.Map(lmiSocDataset);

                if (await graphService.ImportAsync(graphSocDataset).ConfigureAwait(false))
                {
                    return graphSocDataset!.ItemId;
                }
            }

            return null;
        }

        [FunctionName(nameof(PostGraphEventActivity))]
        public async Task PostGraphEventActivity([ActivityTrigger] EventGridPostRequestModel? eventGridPostRequest)
        {
            _ = eventGridPostRequest ?? throw new ArgumentNullException(nameof(eventGridPostRequest));

            logger.LogInformation($"Posting to event grid for: {eventGridPostRequest.DisplayText}: {eventGridPostRequest.EventType}");

            var eventGridEventData = new EventGridEventData
            {
                ItemId = $"{eventGridPostRequest.ItemId}",
                Api = $"{eventGridClientOptions.ApiEndpoint}" + (eventGridPostRequest.ItemId.HasValue ? $"/{eventGridPostRequest.ItemId.Value}" : string.Empty),
                DisplayText = eventGridPostRequest.DisplayText,
                VersionId = Guid.NewGuid().ToString(),
                Author = eventGridClientOptions.SubjectPrefix,
            };

            await eventGridService.SendEventAsync(eventGridEventData, eventGridClientOptions.SubjectPrefix, eventGridPostRequest.EventType).ConfigureAwait(false);
        }
    }
}