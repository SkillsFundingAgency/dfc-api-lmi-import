using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.SocDataset
{
    [ExcludeFromCodeCoverage]
    public class ReplacementDemandModel
    {
        public string? Measure { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        public decimal Rate { get; set; }
    }
}
