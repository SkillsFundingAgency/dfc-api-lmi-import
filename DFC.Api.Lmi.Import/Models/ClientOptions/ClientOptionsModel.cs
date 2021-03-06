﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.ClientOptions
{
    [ExcludeFromCodeCoverage]
    public abstract class ClientOptionsModel
    {
        public Uri? BaseAddress { get; set; }

        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 30);         // default to 30 seconds

        public string? Version { get; set; }

        public string? ApiKey { get; set; }
    }
}
