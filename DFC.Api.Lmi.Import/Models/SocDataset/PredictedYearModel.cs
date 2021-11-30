using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.SocDataset
{
    [ExcludeFromCodeCoverage]
    public class PredictedYearModel
    {
        public int Year { get; set; }

        public decimal Employment { get; set; }
    }
}
