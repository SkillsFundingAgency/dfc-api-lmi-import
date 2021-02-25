using DFC.Api.Lmi.Import.Attributes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSocBreakdownYear")]
    public class GraphBreakdownYearModel : GraphBaseSocModel
    {
        [GraphProperty(nameof(Measure), isKey: true)]
        public string? Measure { get; set; }

        [GraphProperty(nameof(Year), isKey: true, isPreferredLabel: true)]
        public int Year { get; set; }

        [GraphRelationship("BreakdownYears")]
        public List<GraphBreakdownYearValueModel>? Breakdown { get; set; }
    }
}
