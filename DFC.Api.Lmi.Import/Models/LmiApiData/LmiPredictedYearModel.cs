using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.LmiApiData
{
    [ExcludeFromCodeCoverage]
    public class LmiPredictedYearModel
    {
        public int Year { get; set; }

        public decimal Employment { get; set; }
    }
}
