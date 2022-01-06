using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.SocDataset
{
    [ExcludeFromCodeCoverage]
    public class BreakdownYearModel
    {
        public int Year { get; set; }

        public List<BreakdownYearValueModel>? Breakdown { get; set; }
    }
}
