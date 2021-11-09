using DFC.Api.Lmi.Import.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSocReplacementDemand")]
    public class GraphReplacementDemandModel : GraphBaseSocModel
    {
        [GraphProperty(nameof(Measure), isPreferredLabel: true)]
        public string? Measure { get; set; }

        [GraphProperty(nameof(StartYear))]
        public int StartYear { get; set; }

        [GraphProperty(nameof(EndYear))]
        public int EndYear { get; set; }

        [GraphProperty(nameof(Rate))]
        public decimal Rate { get; set; }
    }
}
