using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.SocDataset
{
    [ExcludeFromCodeCoverage]
    public class BreakdownModel
    {
        public string? Note { get; set; }

        public string? Measure { get; set; }

        public List<BreakdownYearModel>? PredictedEmployment { get; set; }
    }
}
