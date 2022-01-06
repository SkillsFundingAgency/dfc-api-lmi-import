using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.SocDataset
{
    [ExcludeFromCodeCoverage]
    public class BreakdownYearValueModel
    {
        public int Code { get; set; }

        public string? Note { get; set; }

        public string? Name { get; set; }

        public decimal Employment { get; set; }
    }
}
