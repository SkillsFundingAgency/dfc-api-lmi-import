using AutoMapper;
using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.FunctionRequestModels;
using DFC.Api.Lmi.Import.Models.SocDataset;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private readonly IMapper mapper;
        private readonly IJobProfileService jobProfileService;
        private readonly ILmiSocImportService lmiSocImportService;
        private readonly IDocumentService<SocDatasetModel> documentService;
        private readonly IEventGridService eventGridService;
        private readonly EventGridClientOptions eventGridClientOptions;
        private readonly SocJobProfilesMappingsCachedModel socJobProfilesMappingsCachedModel;

        public LmiImportOrchestrationTrigger(
            ILogger<LmiImportOrchestrationTrigger> logger,
            IMapper mapper,
            IJobProfileService jobProfileService,
            ILmiSocImportService lmiSocImportService,
            IDocumentService<SocDatasetModel> documentService,
            IEventGridService eventGridService,
            EventGridClientOptions eventGridClientOptions,
            SocJobProfilesMappingsCachedModel socJobProfilesMappingsCachedModel)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.jobProfileService = jobProfileService;
            this.lmiSocImportService = lmiSocImportService;
            this.documentService = documentService;
            this.eventGridService = eventGridService;
            this.eventGridClientOptions = eventGridClientOptions;
            this.socJobProfilesMappingsCachedModel = socJobProfilesMappingsCachedModel;

            //TODO: ian: need to initialize the telemetry properly
            Activity? activity = null;
            if (Activity.Current == null)
            {
                activity = new Activity(nameof(LmiImportOrchestrationTrigger)).Start();
                activity.SetParentId(Guid.NewGuid().ToString());
            }
        }

        [FunctionName(nameof(CacheRefreshSocOrchestrator))]
        public async Task<HttpStatusCode> CacheRefreshSocOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var socRequest = context.GetInput<SocRequestModel>();

            if (socJobProfilesMappingsCachedModel.SocJobProfileMappings == null || !socJobProfilesMappingsCachedModel.SocJobProfileMappings.Any())
            {
                socJobProfilesMappingsCachedModel.SocJobProfileMappings = await context.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(GetJobProfileSocMappingsActivity), null).ConfigureAwait(true);
            }

            var socJobProfileMapping = new SocJobProfileMappingModel { Soc = socRequest.Soc, JobProfiles = socJobProfilesMappingsCachedModel.SocJobProfileMappings.FirstOrDefault(f => f.Soc == socRequest.Soc)?.JobProfiles };
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

                await context.CallActivityAsync(nameof(PostCacheEventActivity), eventGridPostRequest).ConfigureAwait(true);

                return HttpStatusCode.OK;
            }

            return HttpStatusCode.NoContent;
        }

        [FunctionName(nameof(CachePurgeSocOrchestrator))]
        public async Task CachePurgeSocOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var socRequest = context.GetInput<SocRequestModel>();

            var existingDocument = await context.CallActivityAsync<SocDatasetModel>(nameof(GetCachedSocDocumentActivity), socRequest.Soc).ConfigureAwait(true);

            if (existingDocument != null)
            {
                await context.CallActivityAsync(nameof(CachePurgeSocActivity), existingDocument.Id).ConfigureAwait(true);

                var eventGridPostRequest = new EventGridPostRequestModel
                {
                    ItemId = existingDocument.Id,
                    Api = $"{eventGridClientOptions.ApiEndpoint}/{existingDocument.Id}",
                    DisplayText = $"LMI SOC purged: {existingDocument.Soc}",
                    EventType = socRequest.IsDraftEnvironment ? EventTypeForDraftDiscarded : EventTypeForDeleted,
                };

                await context.CallActivityAsync(nameof(PostCacheEventActivity), eventGridPostRequest).ConfigureAwait(true);
            }
        }

        [FunctionName(nameof(CachePurgeOrchestrator))]
        public async Task CachePurgeOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var orchestratorRequestModel = context.GetInput<OrchestratorRequestModel>();
            await context.CallActivityAsync(nameof(CachePurgeActivity), null).ConfigureAwait(true);

            var eventGridPostRequest = new EventGridPostRequestModel
            {
                ItemId = context.NewGuid(),
                Api = $"{eventGridClientOptions.ApiEndpoint}",
                DisplayText = "LMI Import purged",
                EventType = orchestratorRequestModel.IsDraftEnvironment ? EventTypeForDraftDiscarded : EventTypeForDeleted,
            };

            await context.CallActivityAsync(nameof(PostCacheEventActivity), eventGridPostRequest).ConfigureAwait(true);
        }

        [FunctionName(nameof(CacheRefreshOrchestrator))]
        [Timeout("01:00:00")]
        public async Task<HttpStatusCode> CacheRefreshOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            logger.LogInformation("Start importing of LMI data from API");

            var orchestratorRequestModel = context.GetInput<OrchestratorRequestModel>();
            var socJobProfileMappings = await context.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(GetJobProfileSocMappingsActivity), null).ConfigureAwait(true);

            if (socJobProfileMappings != null && socJobProfileMappings.Any())
            {
                await context.CallActivityAsync(nameof(CachePurgeActivity), null).ConfigureAwait(true);

                logger.LogInformation($"Importing {socJobProfileMappings.Count} SOC mappings");

                var parallelTasks = new List<Task<Guid?>>();

                foreach (var socJobProfileMapping in socJobProfileMappings)
                {
                    parallelTasks.Add(context.CallActivityAsync<Guid?>(nameof(ImportSocItemActivity), socJobProfileMapping));
                }

                await Task.WhenAll(parallelTasks).ConfigureAwait(true);

                decimal importedToCacheCount = parallelTasks.Count(t => t.Result != null);
                var successPercentage = decimal.Divide(importedToCacheCount, socJobProfileMappings.Count) * 100;

                logger.LogInformation($"Imported to cache {importedToCacheCount} of {socJobProfileMappings.Count} SOC mappings = {successPercentage:0.0}% success");

                if (successPercentage >= orchestratorRequestModel.SuccessRelayPercent)
                {
                    var eventGridPostRequest = new EventGridPostRequestModel
                    {
                        ItemId = context.NewGuid(),
                        Api = $"{eventGridClientOptions.ApiEndpoint}",
                        DisplayText = "LMI Import refreshed",
                        EventType = orchestratorRequestModel.IsDraftEnvironment ? EventTypeForDraft : EventTypeForPublished,
                    };

                    await context.CallActivityAsync(nameof(PostCacheEventActivity), eventGridPostRequest).ConfigureAwait(true);

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

        [FunctionName(nameof(GetJobProfileSocMappingsActivity))]
        public Task<IList<SocJobProfileMappingModel>?> GetJobProfileSocMappingsActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Getting Job profile to SOC mappings");

            return jobProfileService.GetMappingsAsync();
        }

        [FunctionName(nameof(CachePurgeActivity))]
        public Task CachePurgeActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Purging cache of all SOC");

            return documentService.PurgeAsync();
        }

        [FunctionName(nameof(CachePurgeSocActivity))]
        public Task CachePurgeSocActivity([ActivityTrigger] Guid id)
        {
            logger.LogInformation($"Purging cache of SOC guid: {id}");

            return documentService.DeleteAsync(id);
        }

        [FunctionName(nameof(ImportSocItemActivity))]
        public async Task<Guid?> ImportSocItemActivity([ActivityTrigger] SocJobProfileMappingModel socJobProfileMapping)
        {
            _ = socJobProfileMapping ?? throw new ArgumentNullException(nameof(socJobProfileMapping));

            logger.LogInformation($"Importing SOC: {socJobProfileMapping.Soc}");

            var lmiSocDataset = await lmiSocImportService.ImportAsync(socJobProfileMapping.Soc!.Value, socJobProfileMapping.JobProfiles).ConfigureAwait(false);
            if (lmiSocDataset != null)
            {
                var socDataset = mapper.Map<SocDatasetModel>(lmiSocDataset);
                var exisitingDocument = await documentService.GetAsync(w => w.Soc == lmiSocDataset.Soc, lmiSocDataset.Soc.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
                if (exisitingDocument != null)
                {
                    socDataset.Id = exisitingDocument.Id;
                    socDataset.Etag = exisitingDocument.Etag;
                }

                var upsertResult = await documentService.UpsertAsync(socDataset).ConfigureAwait(false);
                if (upsertResult == HttpStatusCode.OK || upsertResult == HttpStatusCode.Created)
                {
                    return socDataset.Id;
                }
            }

            return null;
        }

        [FunctionName(nameof(GetCachedSocDocumentActivity))]
        public Task<SocDatasetModel?> GetCachedSocDocumentActivity([ActivityTrigger] int soc)
        {
            logger.LogInformation($"Getting cached document for Soc: {soc}");

            return documentService.GetAsync(w => w.Soc == soc, soc.ToString(CultureInfo.InvariantCulture));
        }

        [FunctionName(nameof(PostCacheEventActivity))]
        public Task PostCacheEventActivity([ActivityTrigger] EventGridPostRequestModel? eventGridPostRequest)
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

            return eventGridService.SendEventAsync(eventGridEventData, eventGridClientOptions.SubjectPrefix, eventGridPostRequest.EventType);
        }
    }
}