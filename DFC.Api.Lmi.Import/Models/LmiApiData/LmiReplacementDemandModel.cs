using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.LmiApiData
{
    [ExcludeFromCodeCoverage]
    public class LmiReplacementDemandModel
    {
        public int Soc { get; set; }

        [JsonProperty("start_year")]
        public int StartYear { get; set; }

        [JsonProperty("end_year")]
        public int EndYear { get; set; }

        public decimal Rate { get; set; }
    }
}
