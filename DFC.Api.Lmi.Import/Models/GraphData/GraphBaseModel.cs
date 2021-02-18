using DFC.Api.Lmi.Import.Attributes;
using System;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    public abstract class GraphBaseModel
    {
        [GraphProperty(nameof(Soc), isKey: true)]
        public int Soc { get; set; }

        [GraphProperty(nameof(CreatedDate))]
        public DateTime CreatedDate { get; set; }
    }
}
