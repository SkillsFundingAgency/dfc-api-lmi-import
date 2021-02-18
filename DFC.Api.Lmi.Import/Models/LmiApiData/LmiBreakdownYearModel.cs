using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.LmiApiData
{
    [ExcludeFromCodeCoverage]
    public class LmiBreakdownYearModel
    {
        public int Year { get; set; }

        public IList<LmiBreakdownYearItemModel>? Breakdown { get; set; }
    }
}
