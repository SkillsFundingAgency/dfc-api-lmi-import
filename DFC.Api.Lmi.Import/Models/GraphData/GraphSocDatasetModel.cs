using DFC.Api.Lmi.Import.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSoc")]
    public class GraphSocDatasetModel : GraphBaseModel
    {
        [GraphProperty(nameof(Soc), isKey: true, isPreferredLabel: true)]
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
        public IList<GraphJobProfileModel>? JobProfiles { get; set; }

        [GraphRelationshipRoot("Predicted" + nameof(JobGrowth))]
        public GraphPredictedModel? JobGrowth { get; set; }

        [GraphRelationshipRoot("Breakdown" + nameof(QualificationLevel))]
        public GraphBreakdownModel? QualificationLevel { get; set; }

        [GraphRelationshipRoot("Breakdown" + nameof(EmploymentByRegion))]
        public GraphBreakdownModel? EmploymentByRegion { get; set; }

        [GraphRelationshipRoot("Breakdown" + nameof(TopIndustriesInJobGroup))]
        public GraphBreakdownModel? TopIndustriesInJobGroup { get; set; }
    }
}
