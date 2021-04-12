using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.LmiApiData
{
    [ExcludeFromCodeCoverage]
    public class LmiPredictedModel
    {
        public int Soc { get; set; }

        public List<LmiPredictedYearModel>? PredictedEmployment { get; set; }
    }
}
