using DFC.Api.Lmi.Import.Attributes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSocBreakdown")]
    public class GraphBreakdownModel : GraphBaseModel
    {
        [GraphProperty(nameof(Note))]
        public string? Note { get; set; }

        [GraphProperty(nameof(Measure), isKey: true, isPreferredLabel: true)]
        public string? Measure { get; set; }

        [GraphRelationship("Breakdown" + nameof(PredictedEmployment))]
        public IList<GraphBreakdownYearModel>? PredictedEmployment { get; set; }
    }
}
