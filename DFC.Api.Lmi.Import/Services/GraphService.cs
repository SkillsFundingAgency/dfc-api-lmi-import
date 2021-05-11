using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.GraphData;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    public class GraphService : IGraphService
    {
        private readonly ILogger<GraphService> logger;
        private readonly IGraphConnector graphConnector;
        private readonly ISocGraphQueryService socGraphQueryService;

        public GraphService(
            ILogger<GraphService> logger,
            IGraphConnector graphConnector,
            ISocGraphQueryService socGraphQueryService)
        {
            this.logger = logger;
            this.graphConnector = graphConnector;
            this.socGraphQueryService = socGraphQueryService;
        }

        public async Task<bool> ImportAsync(GraphSocDatasetModel? graphSocDataset, GraphReplicaSet graphReplicaSet)
        {
            _ = graphSocDataset ?? throw new ArgumentNullException(nameof(graphSocDataset));

            try
            {
                logger.LogInformation($"Importing SOC dataset to Graph: {graphSocDataset.Soc}");

                var commands = graphConnector.BuildImportCommands(graphSocDataset);

                logger.LogInformation($"Importing SOC dataset to Graph: {graphSocDataset.Soc}: executing commands");

                await graphConnector.RunAsync(commands, graphReplicaSet).ConfigureAwait(false);

                logger.LogInformation($"Imported SOC dataset to Graph: {graphSocDataset.Soc}");

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error received importing data to graphs database for LMI SOC: {graphSocDataset.Soc}");
                return false;
            }
        }

        public async Task PublishAsync(GraphReplicaSet fromGraphReplicaSet, GraphReplicaSet toGraphReplicaSet)
        {
            logger.LogInformation($"Publishing from {fromGraphReplicaSet} LMI data to {toGraphReplicaSet} graph");

            var socModels = await socGraphQueryService.GetSummaryAsync(fromGraphReplicaSet).ConfigureAwait(false);

            if (socModels != null && socModels.Any())
            {
                foreach (var socModel in socModels)
                {
                    var graphSocDataset = await socGraphQueryService.GetDetailAsync(fromGraphReplicaSet, socModel.Soc).ConfigureAwait(false);
                    if (graphSocDataset != null)
                    {
                        await ImportAsync(graphSocDataset, toGraphReplicaSet).ConfigureAwait(false);
                    }
                }
            }

            logger.LogInformation($"Published from {fromGraphReplicaSet} LMI data to {toGraphReplicaSet} graph");
        }

        public async Task PurgeAsync(GraphReplicaSet graphReplicaSet)
        {
            logger.LogInformation("Purging Graph of LMI data");

            var commands = graphConnector.BuildPurgeCommands();

            logger.LogInformation("Purging Graph of LMI data: executing commands");

            await graphConnector.RunAsync(commands, graphReplicaSet).ConfigureAwait(false);

            logger.LogInformation("Purged Graph of LMI data");
        }

        public async Task PurgeSocAsync(int soc, GraphReplicaSet graphReplicaSet)
        {
            logger.LogInformation($"Purging Graph of LMI data for SOC {soc}");

            var commands = graphConnector.BuildPurgeCommandsForInitialKey(soc.ToString(CultureInfo.InvariantCulture));

            logger.LogInformation($"Purging Graph of LMI data for SOC {soc}: executing commands");

            await graphConnector.RunAsync(commands, graphReplicaSet).ConfigureAwait(false);

            logger.LogInformation($"Purged Graph of LMI data for SOC {soc}");
        }
    }
}
