using DFC.Api.Lmi.Import.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSocJobProfile")]
    public class GraphJobProfileModel : GraphBaseModel
    {
        [GraphProperty(nameof(CanonicalName), isInitialKey: true, isKey: true, isPreferredLabel: true)]
        public string? CanonicalName { get; set; }

        [GraphProperty(nameof(Title))]
        public string? Title { get; set; }
    }
}
