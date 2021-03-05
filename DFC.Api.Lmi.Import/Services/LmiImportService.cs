using DFC.Api.Lmi.Import.Contracts;
using Microsoft.Extensions.Logging;
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

        public LmiImportService(
            ILogger<LmiImportService> logger,
            IMapLmiToGraphService mapLmiToGraphService,
            IJobProfileService jobProfileService,
            ILmiSocImportService lmiSocImportService,
            IGraphService graphService)
        {
            this.logger = logger;
            this.mapLmiToGraphService = mapLmiToGraphService;
            this.jobProfileService = jobProfileService;
            this.lmiSocImportService = lmiSocImportService;
            this.graphService = graphService;
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
                    var lmiSocDataset = await lmiSocImportService.ImportAsync(socJobProfileMapping).ConfigureAwait(false);
                    if (lmiSocDataset != null)
                    {
                        var graphSocDatasetModel = mapLmiToGraphService.Map(lmiSocDataset);

                        if (await graphService.ImportAsync(graphSocDatasetModel).ConfigureAwait(false))
                        {
                            importedToGraphCount++;
                        }
                    }
                }

                logger.LogInformation($"Imported to Graph {importedToGraphCount} of {socJobProfileMappings.Count} SOC mappings");
            }
        }
    }
}
