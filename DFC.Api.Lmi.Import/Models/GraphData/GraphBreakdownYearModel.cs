using DFC.Api.Lmi.Import.Attributes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSocBreakdownYear")]
    public class GraphBreakdownYearModel : GraphBaseModel
    {
        [GraphProperty(nameof(BreakdownType), isKey: true)]
        public string? BreakdownType { get; set; }

        [GraphProperty(nameof(Year), isKey: true, isPreferredLabel: true)]
        public int Year { get; set; }

        [GraphRelationship("BreakdownYears")]
        public IList<GraphBreakdownYearItemModel>? Breakdown { get; set; }
    }
}
