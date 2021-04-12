using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.LmiApiData
{
    [ExcludeFromCodeCoverage]
    public class LmiBreakdownYearModel
    {
        public int Year { get; set; }

        public List<LmiBreakdownYearValueModel>? Breakdown { get; set; }
    }
}
