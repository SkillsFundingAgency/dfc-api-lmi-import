using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.ClientOptions
{
    [ExcludeFromCodeCoverage]
    public class JobProfileApiClientOptions : ClientOptionsModel
    {
        public int DeveloperModeMaxJobProfiles { get; set; } = 0;
    }
}