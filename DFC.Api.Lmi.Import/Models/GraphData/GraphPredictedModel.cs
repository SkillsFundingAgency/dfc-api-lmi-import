using DFC.Api.Lmi.Import.Attributes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSocPredicted")]
    public class GraphPredictedModel : GraphBaseSocModel
    {
        [GraphProperty(nameof(Measure), isPreferredLabel: true)]
        public string? Measure { get; set; }

        [GraphRelationship(nameof(PredictedEmployment))]
        public List<GraphPredictedYearModel>? PredictedEmployment { get; set; }
    }
}
