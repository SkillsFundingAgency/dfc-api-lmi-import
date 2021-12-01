using DFC.Api.Lmi.Import.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DFC.Api.Lmi.Import.Models
{
    [ExcludeFromCodeCoverage]
    public class EnvironmentValues
    {
        public int SuccessRelayPercent { get; set; } = int.Parse(Environment.GetEnvironmentVariable(Constants.EnvironmentNameSuccessRelayPercent) ?? "90", CultureInfo.InvariantCulture);
    }
}
