using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.GraphData;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    public class SocGraphQueryService : ISocGraphQueryService
    {
        private readonly string graphSocDatasetNodeName = Utilities.AttributeUtilities.GetGraphNodeName<GraphSocDatasetModel>() ?? throw new NotImplementedException(nameof(graphSocDatasetNodeName));
        private readonly string graphJobProfileNodeName = Utilities.AttributeUtilities.GetGraphNodeName<GraphJobProfileModel>() ?? throw new NotImplementedException(nameof(graphJobProfileNodeName));
        private readonly string graphPredictedNodeName = Utilities.AttributeUtilities.GetGraphNodeName<GraphPredictedModel>() ?? throw new NotImplementedException(nameof(graphPredictedNodeName));
        private readonly string graphPredictedYearNodeName = Utilities.AttributeUtilities.GetGraphNodeName<GraphPredictedYearModel>() ?? throw new NotImplementedException(nameof(graphPredictedYearNodeName));
        private readonly string graphBreakdownNodeName = Utilities.AttributeUtilities.GetGraphNodeName<GraphBreakdownModel>() ?? throw new NotImplementedException(nameof(graphBreakdownNodeName));
        private readonly string graphBreakdownYearNodeName = Utilities.AttributeUtilities.GetGraphNodeName<GraphBreakdownYearModel>() ?? throw new NotImplementedException(nameof(graphBreakdownYearNodeName));
        private readonly string graphBreakdownYearValueNodeName = Utilities.AttributeUtilities.GetGraphNodeName<GraphBreakdownYearValueModel>() ?? throw new NotImplementedException(nameof(graphBreakdownYearValueNodeName));

        private readonly string graphJobProfileRelationshipName = Utilities.AttributeUtilities.GetGraphRelationshipName<GraphSocDatasetModel>(nameof(GraphSocDatasetModel.JobProfiles)) ?? throw new NotImplementedException(nameof(graphJobProfileRelationshipName));
        private readonly string graphJobGrowthRelationshipName = Utilities.AttributeUtilities.GetGraphRelationshipName<GraphSocDatasetModel>(nameof(GraphSocDatasetModel.JobGrowth)) ?? throw new NotImplementedException(nameof(graphJobGrowthRelationshipName));
        private readonly string graphPredictedEmploymentRelationshipName = Utilities.AttributeUtilities.GetGraphRelationshipName<GraphPredictedModel>(nameof(GraphPredictedModel.PredictedEmployment)) ?? throw new NotImplementedException(nameof(graphPredictedEmploymentRelationshipName));
        private readonly string graphQualificationLevelRelationshipName = Utilities.AttributeUtilities.GetGraphRelationshipName<GraphSocDatasetModel>(nameof(GraphSocDatasetModel.QualificationLevel)) ?? throw new NotImplementedException(nameof(graphQualificationLevelRelationshipName));
        private readonly string graphEmploymentByRegionRelationshipName = Utilities.AttributeUtilities.GetGraphRelationshipName<GraphSocDatasetModel>(nameof(GraphSocDatasetModel.EmploymentByRegion)) ?? throw new NotImplementedException(nameof(graphEmploymentByRegionRelationshipName));
        private readonly string graphTopIndustriesInJobGroupRelationshipName = Utilities.AttributeUtilities.GetGraphRelationshipName<GraphSocDatasetModel>(nameof(GraphSocDatasetModel.TopIndustriesInJobGroup)) ?? throw new NotImplementedException(nameof(graphTopIndustriesInJobGroupRelationshipName));
        private readonly string graphBreakdownRelationshipName = Utilities.AttributeUtilities.GetGraphRelationshipName<GraphBreakdownModel>(nameof(GraphBreakdownModel.PredictedEmployment)) ?? throw new NotImplementedException(nameof(graphBreakdownRelationshipName));
        private readonly string graphBreakdownYearRelationshipName = Utilities.AttributeUtilities.GetGraphRelationshipName<GraphBreakdownYearModel>(nameof(GraphBreakdownYearModel.Breakdown)) ?? throw new NotImplementedException(nameof(graphBreakdownYearRelationshipName));

        private readonly ILogger<SocGraphQueryService> logger;
        private readonly IGenericGraphQueryService genericGraphQueryService;

        public SocGraphQueryService(
            ILogger<SocGraphQueryService> logger,
            IGenericGraphQueryService genericGraphQueryService)
        {
            this.logger = logger;
            this.genericGraphQueryService = genericGraphQueryService;
        }

        public async Task<List<SocModel>> GetSummaryAsync(GraphReplicaSet graphReplicaSet)
        {
            logger.LogInformation($"Fetching Soc summary from {graphReplicaSet}");
            var query = $"MATCH(s:{graphSocDatasetNodeName}) RETURN s";
            var models = await genericGraphQueryService.ExecuteCypherQuery<SocModel>(graphReplicaSet, query).ConfigureAwait(false);

            return models;
        }

        public async Task<GraphSocDatasetModel?> GetDetailAsync(GraphReplicaSet graphReplicaSet, int soc)
        {
            logger.LogInformation($"Fetching Soc ({soc}) details from {graphReplicaSet}");
            var query = $"MATCH(s:{graphSocDatasetNodeName} {{Soc: {soc}}}) RETURN s";
            var graphSocDatasets = await genericGraphQueryService.ExecuteCypherQuery<GraphSocDatasetModel>(graphReplicaSet, query).ConfigureAwait(false);
            var graphSocDataset = graphSocDatasets?.FirstOrDefault();

            if (graphSocDataset != null)
            {
                graphSocDataset.JobProfiles = await GetJobProfilesAsync(graphReplicaSet, soc).ConfigureAwait(false);
                graphSocDataset.JobGrowth = await GetPredictedAsync(graphReplicaSet, soc).ConfigureAwait(false);
                graphSocDataset.QualificationLevel = await GetBreakdownAsync(graphReplicaSet, soc, graphQualificationLevelRelationshipName).ConfigureAwait(false);
                graphSocDataset.EmploymentByRegion = await GetBreakdownAsync(graphReplicaSet, soc, graphEmploymentByRegionRelationshipName).ConfigureAwait(false);
                graphSocDataset.TopIndustriesInJobGroup = await GetBreakdownAsync(graphReplicaSet, soc, graphTopIndustriesInJobGroupRelationshipName).ConfigureAwait(false);
            }

            return graphSocDataset;
        }

        public async Task<List<GraphJobProfileModel>?> GetJobProfilesAsync(GraphReplicaSet graphReplicaSet, int soc)
        {
            logger.LogInformation($"Fetching Soc ({soc}) {graphJobProfileNodeName} from {graphReplicaSet}");
            var query = $"MATCH({graphSocDatasetNodeName} {{Soc: {0}}}) -[r:{graphJobProfileRelationshipName}]-> (n:{graphJobProfileNodeName}) RETURN n";
            var models = await genericGraphQueryService.ExecuteCypherQuery<GraphJobProfileModel>(graphReplicaSet, query).ConfigureAwait(false);

            return models;
        }

        public async Task<GraphPredictedModel?> GetPredictedAsync(GraphReplicaSet graphReplicaSet, int soc)
        {
            logger.LogInformation($"Fetching Soc ({soc}) {graphPredictedNodeName} from {graphReplicaSet}");
            var query = $"MATCH({graphSocDatasetNodeName} {{Soc: {soc}}}) -[r:{graphJobGrowthRelationshipName}]-> (n:{graphPredictedNodeName}) RETURN n";
            var models = await genericGraphQueryService.ExecuteCypherQuery<GraphPredictedModel>(graphReplicaSet, query).ConfigureAwait(false);
            var model = models?.FirstOrDefault();

            if (model != null)
            {
                model.PredictedEmployment = await GetPredictedYearAsync(graphReplicaSet, soc).ConfigureAwait(false);
            }

            return model;
        }

        public async Task<List<GraphPredictedYearModel>?> GetPredictedYearAsync(GraphReplicaSet graphReplicaSet, int soc)
        {
            logger.LogInformation($"Fetching Soc ({soc}) {graphPredictedYearNodeName} from {graphReplicaSet}");
            var query = $"MATCH({graphSocDatasetNodeName} {{Soc: {soc}}}) -[{graphJobGrowthRelationshipName}]-> ({graphPredictedNodeName}) -[{graphPredictedEmploymentRelationshipName}]-> (n:{graphPredictedYearNodeName}) RETURN n";
            var models = await genericGraphQueryService.ExecuteCypherQuery<GraphPredictedYearModel>(graphReplicaSet, query).ConfigureAwait(false);

            return models;
        }

        public async Task<GraphBreakdownModel?> GetBreakdownAsync(GraphReplicaSet graphReplicaSet, int soc, string graphRelationshipName)
        {
            logger.LogInformation($"Fetching Soc ({soc}) {graphBreakdownNodeName} from {graphReplicaSet}");
            var query = $"MATCH({graphSocDatasetNodeName} {{Soc: {soc}}}) -[r:{graphRelationshipName}]-> (n:{graphBreakdownNodeName}) RETURN n";
            var models = await genericGraphQueryService.ExecuteCypherQuery<GraphBreakdownModel>(graphReplicaSet, query).ConfigureAwait(false);

            var model = models?.FirstOrDefault();

            if (model != null)
            {
                model.PredictedEmployment = await GetBreakdownYearAsync(graphReplicaSet, soc, graphRelationshipName).ConfigureAwait(false);
            }

            return model;
        }

        public async Task<List<GraphBreakdownYearModel>?> GetBreakdownYearAsync(GraphReplicaSet graphReplicaSet, int soc, string graphRelationshipName)
        {
            logger.LogInformation($"Fetching Soc ({soc}) {graphRelationshipName}/{graphBreakdownRelationshipName} from {graphReplicaSet}");
            var query = $"MATCH({graphSocDatasetNodeName} {{Soc: {soc}}}) -[r:{graphRelationshipName}]-> (n:{graphBreakdownNodeName}) -[r2:{graphBreakdownRelationshipName}]-> (n2:{graphBreakdownYearNodeName}) RETURN n2";
            var models = await genericGraphQueryService.ExecuteCypherQuery<GraphBreakdownYearModel>(graphReplicaSet, query).ConfigureAwait(false);

            if (models != null && models.Any())
            {
                foreach (var model in models)
                {
                    model.Breakdown = await GetBreakdownYearValueAsync(graphReplicaSet, soc, graphRelationshipName).ConfigureAwait(false);
                }
            }

            return models;
        }

        public async Task<List<GraphBreakdownYearValueModel>?> GetBreakdownYearValueAsync(GraphReplicaSet graphReplicaSet, int soc, string graphRelationshipName)
        {
            logger.LogInformation($"Fetching Soc ({soc}) {graphRelationshipName}/{graphBreakdownRelationshipName}/{graphBreakdownYearRelationshipName} from {graphReplicaSet}");
            var query = $"MATCH({graphSocDatasetNodeName} {{Soc: {soc}}}) -[r:{graphRelationshipName}]-> (n:{graphBreakdownNodeName}) -[r2:{graphBreakdownRelationshipName}]-> (n2:{graphBreakdownYearNodeName}) -[r3:{graphBreakdownYearRelationshipName}]-> (n3:{graphBreakdownYearValueNodeName}) RETURN n3";
            var models = await genericGraphQueryService.ExecuteCypherQuery<GraphBreakdownYearValueModel>(graphReplicaSet, query).ConfigureAwait(false);

            return models;
        }
    }
}