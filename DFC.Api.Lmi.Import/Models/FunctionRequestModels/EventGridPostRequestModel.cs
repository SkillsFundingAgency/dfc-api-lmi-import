using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.FunctionRequestModels
{
    [ExcludeFromCodeCoverage]
    public class EventGridPostRequestModel
    {
        public int? Soc { get; set; }

        public string? DisplayText { get; set; }

        public string? EventType { get; set; }
    }
}
