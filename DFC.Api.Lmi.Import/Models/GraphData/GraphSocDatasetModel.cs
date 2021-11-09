using DFC.Api.Lmi.Import.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSoc")]
    public class GraphSocDatasetModel : GraphBaseSocModel
    {
        [GraphProperty(nameof(Soc), isInitialKey: true, isKey: true, isPreferredLabel: true)]
        public new int Soc { get; set; }

        [GraphProperty(nameof(Title))]
        public string? Title { get; set; }

        [GraphProperty(nameof(Description))]
        public string? Description { get; set; }

        [GraphProperty(nameof(Qualifications))]
        public string? Qualifications { get; set; }

        [GraphProperty(nameof(Tasks))]
        public string? Tasks { get; set; }

        [JsonProperty(PropertyName = "add_titles")]
        [GraphProperty(nameof(AdditionalTitles), ignore: true)]
        public List<string>? AdditionalTitles { get; set; }

        [GraphRelationship("LinkedTo")]
        public List<GraphJobProfileModel>? JobProfiles { get; set; }

        [GraphRelationship("Predicted" + nameof(JobGrowth))]
        public GraphPredictedModel? JobGrowth { get; set; }

        [GraphRelationship("Replacement" + nameof(ReplacementDemand))]
        public GraphReplacementDemandModel? ReplacementDemand { get; set; }

        [GraphRelationship("Breakdown" + nameof(QualificationLevel))]
        public GraphBreakdownModel? QualificationLevel { get; set; }

        [GraphRelationship("Breakdown" + nameof(EmploymentByRegion))]
        public GraphBreakdownModel? EmploymentByRegion { get; set; }

        [GraphRelationship("Breakdown" + nameof(TopIndustriesInJobGroup))]
        public GraphBreakdownModel? TopIndustriesInJobGroup { get; set; }
    }
}
