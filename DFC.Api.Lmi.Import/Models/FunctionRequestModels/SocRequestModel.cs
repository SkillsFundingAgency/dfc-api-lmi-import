using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.FunctionRequestModels
{
    [ExcludeFromCodeCoverage]
    public class SocRequestModel : OrchestratorRequestModel
    {
        public int Soc { get; set; }
    }
}
