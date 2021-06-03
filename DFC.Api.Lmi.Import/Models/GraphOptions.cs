using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models
{
    [ExcludeFromCodeCoverage]
    public class GraphOptions
    {
        public string PreferredLabelName { get; set; } = "skos__prefLabel";

        public string UriPropertyName { get; set; } = "uri";

        public Uri? ContentApiUriPrefix { get; set; }

        public string PublishedReplicaSetName { get; set; } = "neo4j";

        public string DraftReplicaSetName { get; set; } = "preview";
    }
}
