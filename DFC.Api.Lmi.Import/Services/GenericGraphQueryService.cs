using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    public class GenericGraphQueryService : IGenericGraphQueryService
    {
        //   private readonly IOptionsMonitor<ContentApiOptions> _contentApiOptions;
        private readonly IGraphCluster graphCluster;
        //    private readonly IJsonFormatHelper _jsonFormatHelper;

        // public GenericGraphQueryService(IOptionsMonitor<ContentApiOptions> contentApiOptions, IGraphClusterBuilder graphClusterBuilder, IJsonFormatHelper jsonFormatHelper)
        public GenericGraphQueryService(IGraphClusterBuilder graphClusterBuilder)
        {
            //      _contentApiOptions = contentApiOptions ?? throw new ArgumentNullException(nameof(contentApiOptions));
            graphCluster = graphClusterBuilder.Build() ?? throw new ArgumentNullException(nameof(graphClusterBuilder));
            //        _jsonFormatHelper = jsonFormatHelper ?? throw new ArgumentNullException(nameof(jsonFormatHelper));
        }

        public async Task<IEnumerable<IRecord>> ExecuteCypherQuery(string query)
        {
            return await graphCluster.Run("target", new GenericCypherQueryModel(query)).ConfigureAwait(false);
        }
    }
}
