using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.LmiApiData
{
    [ExcludeFromCodeCoverage]
    public class LmiBreakdownModel
    {
        public int Soc { get; set; }

        public string? Note { get; set; }

        public string? Breakdown { get; set; }

        public IList<LmiBreakdownYearModel>? PredictedEmployment { get; set; }
    }
}
