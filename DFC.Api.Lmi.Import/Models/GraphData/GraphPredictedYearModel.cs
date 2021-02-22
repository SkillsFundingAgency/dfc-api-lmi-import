using DFC.Api.Lmi.Import.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSocPredictedYear")]
    public class GraphPredictedYearModel : GraphBaseSocModel
    {
        [GraphProperty(nameof(Measure))]
        public string? Measure { get; set; }

        [GraphProperty(nameof(Year), isKey: true, isPreferredLabel: true)]
        public int Year { get; set; }

        [GraphProperty(nameof(Employment))]
        public decimal Employment { get; set; }
    }
}
