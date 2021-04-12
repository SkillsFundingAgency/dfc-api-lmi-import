using DFC.Api.Lmi.Import.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.GraphData
{
    [ExcludeFromCodeCoverage]
    public abstract class GraphBaseSocModel : GraphBaseModel
    {
        [GraphProperty(nameof(Soc), isInitialKey: true, isKey: true)]
        public int Soc { get; set; }
    }
}
