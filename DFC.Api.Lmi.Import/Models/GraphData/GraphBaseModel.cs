using DFC.Api.Lmi.Import.Attributes;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    public abstract class GraphBaseModel
    {
        [GraphProperty(nameof(CreatedDate))]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [GraphProperty(nameof(ItemId), ignore: true)]
        public Guid ItemId { get; set; } = Guid.NewGuid();
    }
}
