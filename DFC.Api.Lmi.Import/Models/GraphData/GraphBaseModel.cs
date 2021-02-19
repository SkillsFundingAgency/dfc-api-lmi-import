using DFC.Api.Lmi.Import.Attributes;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    public abstract class GraphBaseModel
    {
        [GraphProperty(nameof(Soc), isKey: true)]
        public int Soc { get; set; }

        [GraphProperty(nameof(CreatedDate))]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
