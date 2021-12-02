using DFC.Compui.Cosmos.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DFC.Api.Lmi.Import.Models.SocDataset
{
    [ExcludeFromCodeCoverage]
    public class SocDatasetModel : DocumentModel
    {
        public override string? PartitionKey
        {
            get => Soc.ToString(CultureInfo.InvariantCulture);

            set => Soc = int.Parse(value ?? "0", CultureInfo.InvariantCulture);
        }

        public int Soc { get; set; }

        public DateTime CachedDate { get; set; } = DateTime.UtcNow;

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Qualifications { get; set; }

        public string? Tasks { get; set; }

        public List<string>? AdditionalTitles { get; set; }

        public List<JobProfileModel>? JobProfiles { get; set; }

        public PredictedModel? JobGrowth { get; set; }

        public ReplacementDemandModel? ReplacementDemand { get; set; }

        public BreakdownModel? QualificationLevel { get; set; }

        public BreakdownModel? EmploymentByRegion { get; set; }

        public BreakdownModel? TopIndustriesInJobGroup { get; set; }
    }
}
