using DFC.Api.Lmi.Import.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.ClientOptions
{
    [ExcludeFromCodeCoverage]
    public class LmiApiClientOptions : ClientOptionsModel
    {
        public IDictionary<LmiApiQuery, string> ApiCalls { get; set; } = new Dictionary<LmiApiQuery, string>
        {
            { LmiApiQuery.SocDetail, "soc/code/{soc}" },
            { LmiApiQuery.JobGrowth, "wf/predict?soc={soc}&minYear={minYear}&maxYear={maxYear}" },
            { LmiApiQuery.QualificationLevel, "wf/predict/breakdown/qualification?soc={soc}&minYear={minYear}&maxYear={maxYear}" },
            { LmiApiQuery.EmploymentByRegion, "wf/predict/breakdown/region?soc={soc}&minYear={minYear}&maxYear={maxYear}" },
            { LmiApiQuery.TopIndustriesInJobGroup, "wf/predict/breakdown/industry?soc={v}&minYear={minYear}&maxYear={maxYear}" },
        };

        public int MinYear { get; set; } = 2020;

        public int MaxYear { get; set; } = 2027;

        public Uri BuildApiUri(int soc, LmiApiQuery lmiApiQuery)
        {
            var apiCall = ApiCalls[lmiApiQuery];
            var query = apiCall.Replace($"{{{nameof(soc)}}}", $"{soc}", StringComparison.OrdinalIgnoreCase)
                               .Replace($"{{{nameof(MinYear)}}}", $"{MinYear}", StringComparison.OrdinalIgnoreCase)
                               .Replace($"{{{nameof(MaxYear)}}}", $"{MaxYear}", StringComparison.OrdinalIgnoreCase);

            var url = BaseAddress + query;

            return new Uri(url, UriKind.Absolute);
        }
    }
}