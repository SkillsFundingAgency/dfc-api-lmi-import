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

            logger.LogInformation($"Importing SOC dataset to Cosmos: {graphSocDataset.Soc}");

            var command = graphConnector.BuildCommand(graphSocDataset);

            logger.LogInformation($"Importing SOC dataset to Cosmos: {graphSocDataset.Soc}: executing command");

            await graphConnector.RunAsync(command, graphSocDataset.Soc).ConfigureAwait(false);

            logger.LogInformation($"Imported SOC dataset to Cosmos: {graphSocDataset.Soc}");
        }
    }
}
