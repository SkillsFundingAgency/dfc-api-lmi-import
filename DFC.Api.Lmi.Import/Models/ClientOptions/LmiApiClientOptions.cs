using DFC.Api.Lmi.Import.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.ClientOptions
{
    [ExcludeFromCodeCoverage]
    public class LmiApiClientOptions : ClientOptionsModel
    {
        public IDictionary<LmiApiQuery, string>? ApiCalls { get; set; } = new Dictionary<LmiApiQuery, string>
        {
            { LmiApiQuery.SocDetail, "soc/code/{soc}" },
            { LmiApiQuery.JobGrowth, "wf/predict?soc={soc}&minYear={minYear}&maxYear={maxYear}" },
            { LmiApiQuery.ReplacementDemand, "wf/replacement_demand?soc={soc}" },
            { LmiApiQuery.QualificationLevel, "wf/predict/breakdown/qualification?soc={soc}&minYear={minYear}&maxYear={minYear}" },
            { LmiApiQuery.EmploymentByRegion, "wf/predict/breakdown/region?soc={soc}&minYear={minYear}&maxYear={minYear}" },
            { LmiApiQuery.TopIndustriesInJobGroup, "wf/predict/breakdown/industry?soc={soc}&minYear={minYear}&maxYear={minYear}" },
        };

        public int MinYear { get; set; } = DateTime.UtcNow.Year;

        public int MaxYear { get; set; } = DateTime.UtcNow.Year + 6;

        public Uri BuildApiUri(int soc, int minYear, int maxYear, LmiApiQuery lmiApiQuery)
        {
            var apiCall = ApiCalls![lmiApiQuery];
            var query = apiCall.Replace($"{{{nameof(soc)}}}", $"{soc}", StringComparison.OrdinalIgnoreCase)
                               .Replace($"{{{nameof(minYear)}}}", $"{minYear}", StringComparison.OrdinalIgnoreCase)
                               .Replace($"{{{nameof(maxYear)}}}", $"{maxYear}", StringComparison.OrdinalIgnoreCase);

            var url = BaseAddress + query;

            return new Uri(url, UriKind.Absolute);
        }
    }
}