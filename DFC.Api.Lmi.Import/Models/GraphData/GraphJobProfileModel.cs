using DFC.Api.Lmi.Import.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("jp", "LmiSocJobProfile")]
    public class GraphJobProfileModel
    {
        [GraphProperty(nameof(CanonicalName), isKey: true, isPreferredLabel: true)]
        public string? CanonicalName { get; set; }

        [GraphProperty(nameof(Title))]
        public string? Title { get; set; }
    }
}
