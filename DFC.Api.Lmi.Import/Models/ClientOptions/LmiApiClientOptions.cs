using System;
using System.Collections.Generic;

namespace DFC.Api.Lmi.Import.Models.ClientOptions
{
    public class LmiApiClientOptions : ClientOptionsModel
    {
        public enum LmiQuery
        {
            SocDetail,
            JobGrowth,
            QualificationLevel,
            EmploymentByRegion,
            TopIndustriesInJobGroup,
        }

        public IDictionary<LmiQuery, string> ApiCalls { get; set; } = new Dictionary<LmiQuery, string>
        {
            { LmiQuery.SocDetail, "soc/code/{soc}" },
            { LmiQuery.JobGrowth, "wf/predict?soc={soc}&minYear={minYear}&maxYear={maxYear}" },
            { LmiQuery.QualificationLevel, "wf/predict/breakdown/qualification?soc={soc}&minYear={minYear}&maxYear={maxYear}" },
            { LmiQuery.EmploymentByRegion, "wf/predict/breakdown/region?soc={soc}&minYear={minYear}&maxYear={maxYear}" },
            { LmiQuery.TopIndustriesInJobGroup, "wf/predict/breakdown/industry?soc={v}&minYear={minYear}&maxYear={maxYear}" },
        };

        public int MinYear { get; set; } = 2019;

        public int MaxYear { get; set; } = 2026;

        public Uri BuildApiUri(int soc, LmiQuery lmiQuery)
        {
            var apiCall = ApiCalls[lmiQuery];
            var query = apiCall.Replace($"{{{nameof(soc)}}}", $"{soc}", StringComparison.OrdinalIgnoreCase)
                               .Replace($"{{{nameof(MinYear)}}}", $"{MinYear}", StringComparison.OrdinalIgnoreCase)
                               .Replace($"{{{nameof(MaxYear)}}}", $"{MaxYear}", StringComparison.OrdinalIgnoreCase);

            var url = BaseAddress + query;

            return new Uri(url, UriKind.Absolute);
        }
    }
}