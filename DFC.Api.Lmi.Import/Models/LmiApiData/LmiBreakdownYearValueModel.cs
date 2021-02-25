using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.LmiApiData
{
    [ExcludeFromCodeCoverage]
    public class LmiBreakdownYearValueModel
    {
        public int Code { get; set; }

        public string? Note { get; set; }

        public string? Name { get; set; }

        public decimal Employment { get; set; }
    }
}
