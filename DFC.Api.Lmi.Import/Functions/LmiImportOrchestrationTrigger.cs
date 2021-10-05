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
using System.Net;
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
        private readonly SocJobProfilesMappingsCachedModel socJobProfilesMappingsCachedModel;

        public LmiImportOrchestrationTrigger(
            ILogger<LmiImportOrchestrationTrigger> logger,
            IJobProfileService jobProfileService,
            IMapLmiToGraphService mapLmiToGraphService,
            ILmiSocImportService lmiSocImportService,
            IGraphService graphService,
            IEventGridService eventGridService,
            EventGridClientOptions eventGridClientOptions,
            SocJobProfilesMappingsCachedModel socJobProfilesMappingsCachedModel)
        {
            this.logger = logger;
            this.jobProfileService = jobProfileService;
            this.mapLmiToGraphService = mapLmiToGraphService;
            this.lmiSocImportService = lmiSocImportService;
            this.graphService = graphService;
            this.eventGridService = eventGridService;
            this.eventGridClientOptions = eventGridClientOptions;
            this.socJobProfilesMappingsCachedModel = socJobProfilesMappingsCachedModel;
        }

        [FunctionName(nameof(GraphRefreshSocOrchestrator))]
        public async Task<HttpStatusCode> GraphRefreshSocOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var socRequest = context.GetInput<SocRequestModel>();

            if (socJobProfilesMappingsCachedModel.SocJobProfileMappings == null || !socJobProfilesMappingsCachedModel.SocJobProfileMappings.Any())
            {
                socJobProfilesMappingsCachedModel.SocJobProfileMappings = await context.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(GetJobProfileSocMappingsActivity), null).ConfigureAwait(true);
            }

            var socJobProfileMapping = new SocJobProfileMappingModel { Soc = socRequest.Soc, JobProfiles = socJobProfilesMappingsCachedModel.SocJobProfileMappings.FirstOrDefault(f => f.Soc == socRequest.Soc)?.JobProfiles };

            await context.CallActivityAsync(nameof(GraphPurgeSocActivity), socRequest.Soc).ConfigureAwait(true);

            var eventGridPostPurgeRequest = new EventGridPostRequestModel
            {
                ItemId = socRequest.SocId,
                Api = $"{eventGridClientOptions.ApiEndpoint}/{socRequest.SocId}",
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
                    Api = $"{eventGridClientOptions.ApiEndpoint}/{itemId}",
                    DisplayText = $"LMI SOC refreshed: {socRequest.Soc}",
                    EventType = socRequest.IsDraftEnvironment ? EventTypeForDraft : EventTypeForPublished,
                };

                await context.CallActivityAsync(nameof(PostGraphEventActivity), eventGridPostRequest).ConfigureAwait(true);

                return HttpStatusCode.OK;
            }

            return HttpStatusCode.NoContent;
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
                Api = $"{eventGridClientOptions.ApiEndpoint}/{socRequest.SocId}",
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
                ItemId = context.NewGuid(),
                Api = $"{eventGridClientOptions.ApiEndpoint}",
                DisplayText = "LMI Import purged",
                EventType = orchestratorRequestModel.IsDraftEnvironment ? EventTypeForDraftDiscarded : EventTypeForDeleted,
            };

            await context.CallActivityAsync(nameof(PostGraphEventActivity), eventGridPostRequest).ConfigureAwait(true);
        }

        [FunctionName(nameof(GraphRefreshOrchestrator))]
        [Timeout("01:00:00")]
        public async Task<HttpStatusCode> GraphRefreshOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
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

                decimal importedToGraphCount = parallelTasks.Count(t => t.Result != null);
                var successPercentage = decimal.Divide(importedToGraphCount, socJobProfileMappings.Count) * 100;

                logger.LogInformation($"Imported to Graph {importedToGraphCount} of {socJobProfileMappings.Count} SOC mappings = {successPercentage:0.0}% success");

                if (successPercentage >= orchestratorRequestModel.SuccessRelayPercent)
                {
                    var eventGridPostRequest = new EventGridPostRequestModel
                    {
                        ItemId = context.NewGuid(),
                        Api = $"{eventGridClientOptions.ApiEndpoint}",
                        DisplayText = "LMI Import refreshed",
                        EventType = orchestratorRequestModel.IsDraftEnvironment ? EventTypeForDraft : EventTypeForPublished,
                    };

                    await context.CallActivityAsync(nameof(PostGraphEventActivity), eventGridPostRequest).ConfigureAwait(true);

                    return HttpStatusCode.OK;
                }

                return HttpStatusCode.BadRequest;
            }
            else
            {
                logger.LogWarning("No data available from JOB profile SOC mappings - no data imported");
                return HttpStatusCode.NoContent;
            }
        }

        [FunctionName(nameof(RefreshPublishedOrchestrator))]
        public async Task RefreshPublishedOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            await context.CallActivityAsync(nameof(PurgePublishedActivity), null).ConfigureAwait(true);
            await context.CallActivityAsync(nameof(RefreshPublishedActivity), null).ConfigureAwait(true);
        }

        [FunctionName(nameof(PurgePublishedOrchestrator))]
        public async Task PurgePublishedOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            await context.CallActivityAsync(nameof(PurgePublishedActivity), null).ConfigureAwait(true);
        }

        [FunctionName(nameof(GetJobProfileSocMappingsActivity))]
        public async Task<IList<SocJobProfileMappingModel>?> GetJobProfileSocMappingsActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Getting Job profile to SOC mappings");

            return await jobProfileService.GetMappingsAsync().ConfigureAwait(false);
        }

        [FunctionName(nameof(RefreshPublishedActivity))]
        public async Task RefreshPublishedActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Refreshing published Graph from draft Graph");

            await graphService.PublishAsync(GraphReplicaSet.Draft, GraphReplicaSet.Published).ConfigureAwait(false);
        }

        [FunctionName(nameof(PurgePublishedActivity))]
        public async Task PurgePublishedActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Purging published Graph of all SOC");

            await graphService.PurgeAsync(GraphReplicaSet.Published).ConfigureAwait(false);
        }

        [FunctionName(nameof(GraphPurgeActivity))]
        public async Task GraphPurgeActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Purging draft Graph of all SOC");

            await graphService.PurgeAsync(GraphReplicaSet.Draft).ConfigureAwait(false);
        }

        [FunctionName(nameof(GraphPurgeSocActivity))]
        public async Task GraphPurgeSocActivity([ActivityTrigger] int soc)
        {
            logger.LogInformation($"Purging draft Graph of SOC: {soc}");

            await graphService.PurgeSocAsync(soc, GraphReplicaSet.Draft).ConfigureAwait(false);
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

                if (await graphService.ImportAsync(graphSocDataset, GraphReplicaSet.Draft).ConfigureAwait(false))
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
                Api = eventGridPostRequest.Api,
                DisplayText = eventGridPostRequest.DisplayText,
                VersionId = Guid.NewGuid().ToString(),
                Author = eventGridClientOptions.SubjectPrefix,
            };

            await eventGridService.SendEventAsync(eventGridEventData, eventGridClientOptions.SubjectPrefix, eventGridPostRequest.EventType).ConfigureAwait(false);
        }
    }
}