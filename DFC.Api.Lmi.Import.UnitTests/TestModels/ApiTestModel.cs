using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.UnitTests.TestModels
{
    [ExcludeFromCodeCoverage]
    public class ApiTestModel
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }
    }
}
