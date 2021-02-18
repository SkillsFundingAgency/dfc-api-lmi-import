using DFC.Api.Lmi.Import.Attributes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSocPredicted")]
    public class GraphPredictedModel : GraphBaseModel
    {
        [GraphProperty(nameof(PredictedType), isPreferredLabel: true)]
        public string? PredictedType { get; set; }

        [GraphRelationship(nameof(PredictedEmployment))]
        public IList<GraphPredictedYearModel>? PredictedEmployment { get; set; }
    }
}
