using DFC.Api.Lmi.Import.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("byi", "LmiSocBreakdownYearItem")]
    public class GraphBreakdownYearItemModel : GraphBaseModel
    {
        [GraphProperty(nameof(BreakdownType), isKey: true)]
        public string? BreakdownType { get; set; }

        [GraphProperty(nameof(Year), isKey: true)]
        public int Year { get; set; }

        [GraphProperty(nameof(Code), isKey: true)]
        public int Code { get; set; }

        [GraphProperty(nameof(Note))]
        public string? Note { get; set; }

        [GraphProperty(nameof(Name), isPreferredLabel: true)]
        public string? Name { get; set; }

        [GraphProperty(nameof(Employment))]
        public decimal Employment { get; set; }
    }
}
