using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    [ExcludeFromCodeCoverage]
    public class GenericGraphQueryService : IGenericGraphQueryService
    {
        private readonly IGraphCluster graphCluster;
        private readonly GraphOptions graphOptions;

        public GenericGraphQueryService(IGraphClusterBuilder graphClusterBuilder, GraphOptions graphOptions)
        {
            graphCluster = graphClusterBuilder?.Build() ?? throw new ArgumentNullException(nameof(graphClusterBuilder));
            this.graphOptions = graphOptions;
        }

        public async Task<List<TModel>> ExecuteCypherQuery<TModel>(GraphReplicaSet graphReplicaSet, string query)
            where TModel : class, new()
        {
            string replicaSetName = graphReplicaSet switch
            {
                GraphReplicaSet.Published => graphOptions.PublishedReplicaSetName,
                GraphReplicaSet.Draft => graphOptions.DraftReplicaSetName,
                _ => throw new NotImplementedException(),
            };

            var result = await graphCluster.Run(replicaSetName, new GenericCypherQueryModel<TModel>(query)).ConfigureAwait(false);

            return result;
        }
    }
}
