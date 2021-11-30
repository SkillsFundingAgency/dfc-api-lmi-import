using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.SocDataset
{
    [ExcludeFromCodeCoverage]
    public class PredictedModel
    {
        public string? Measure { get; set; }

        public List<PredictedYearModel>? PredictedEmployment { get; set; }
    }
}
