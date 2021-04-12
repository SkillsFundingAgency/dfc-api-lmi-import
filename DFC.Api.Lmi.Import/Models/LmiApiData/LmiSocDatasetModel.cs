using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.LmiApiData
{
    [ExcludeFromCodeCoverage]
    public class LmiSocDatasetModel
    {
        public int Soc { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Qualifications { get; set; }

        public string? Tasks { get; set; }

        [JsonProperty(PropertyName = "add_titles")]
        public List<string>? AdditionalTitles { get; set; }

        public List<SocJobProfileItemModel>? JobProfiles { get; set; }

        public LmiPredictedModel? JobGrowth { get; set; }

        public LmiBreakdownModel? QualificationLevel { get; set; }

        public LmiBreakdownModel? EmploymentByRegion { get; set; }

        public LmiBreakdownModel? TopIndustriesInJobGroup { get; set; }
    }
}
