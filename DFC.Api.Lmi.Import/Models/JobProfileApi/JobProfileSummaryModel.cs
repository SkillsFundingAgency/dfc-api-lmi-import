using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.JobProfileApi
{
    [ExcludeFromCodeCoverage]
    public class JobProfileSummaryModel
    {
        public Uri? Url { get; set; }

        public string? Title { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
