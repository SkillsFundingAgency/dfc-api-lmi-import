using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.GraphData;
using Microsoft.Extensions.Logging;
using System;
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

        public async Task ImportAsync(GraphSocDatasetModel graphSocDataset)
        {
            _ = graphSocDataset ?? throw new ArgumentNullException(nameof(graphSocDataset));

            logger.LogInformation($"Importing SOC dataset to Graph: {graphSocDataset.Soc}");

            var commands = graphConnector.BuildImportCommands(graphSocDataset);

            logger.LogInformation($"Importing SOC dataset to Graph: {graphSocDataset.Soc}: executing commands");

            await graphConnector.RunAsync(commands).ConfigureAwait(false);

            logger.LogInformation($"Imported SOC dataset to Graph: {graphSocDataset.Soc}");
        }

        public async Task PurgeAsync()
        {
            logger.LogInformation("Purging Graph of LMI data");

            var commands = graphConnector.BuildPurgeCommands();

            logger.LogInformation("Purging Graph of LMI data: executing commands");

            await graphConnector.RunAsync(commands).ConfigureAwait(false);

            logger.LogInformation("Purged Graph of LMI data");
        }
    }
}
