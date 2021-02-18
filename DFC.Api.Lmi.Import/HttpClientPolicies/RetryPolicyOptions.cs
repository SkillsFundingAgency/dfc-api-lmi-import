using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.HttpClientPolicies
{
    [ExcludeFromCodeCoverage]
    public class RetryPolicyOptions
    {
        public int Count { get; set; } = 5;

        public int BackoffPower { get; set; } = 2;
    }
}
