using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.GraphData;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    public class GraphService : IGraphService
    {
        private readonly ILogger<GraphService> logger;
        private readonly IGraphConnector graphConnector;

        public GraphService(
            ILogger<GraphService> logger,
            IGraphConnector graphConnector)
        {
            this.logger = logger;
            this.graphConnector = graphConnector;
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

        public async Task PublishFromDraftAsync(GraphReplicaSet graphReplicaSet)
        {
            logger.LogInformation("Publishing draft LMI data to published graph");

            //var commands = graphConnector.BuildPublishCommands();

            //logger.LogInformation("Publishing draft LMI data to published graph: executing commands");

            //await graphConnector.RunAsync(commands, graphReplicaSet).ConfigureAwait(false);

            logger.LogInformation("Published draft LMI data to published graph");
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
