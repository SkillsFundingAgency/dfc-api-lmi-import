using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.JobProfileApi
{
    [ExcludeFromCodeCoverage]
    public class JobProfileDetailModel
    {
        public int? Soc { get; set; }

        public Uri? Url { get; set; }

        public string? CanonicalName { get; set; }

        public string? Title { get; set; }
    }
}
