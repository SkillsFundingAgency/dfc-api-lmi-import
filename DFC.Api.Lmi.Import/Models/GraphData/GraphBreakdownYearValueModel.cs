using DFC.Api.Lmi.Import.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    [GraphNode("LmiSocBreakdownYearValue")]
    public class GraphBreakdownYearValueModel : GraphBaseSocModel
    {
        [GraphProperty(nameof(Measure), isKey: true)]
        public string? Measure { get; set; }

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
