using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.SocDataset
{
    [ExcludeFromCodeCoverage]
    public class SocDatasetSummaryItemModel
    {
        public Guid? Id { get; set; }

        public int Soc { get; set; }

        public string? Title { get; set; }
    }
}
