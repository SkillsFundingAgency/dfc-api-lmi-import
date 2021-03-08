using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Attributes
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property)]
    public class GraphPropertyAttribute : Attribute
    {
        public GraphPropertyAttribute(string name, bool isPreferredLabel = false, bool isInitialKey = false, bool isKey = false, bool ignore = false)
        {
            Name = name;
            IsPreferredLabel = isPreferredLabel;
            IsInitialKey = isInitialKey;
            IsKey = isKey;
            Ignore = ignore;
        }

        public string Name { get; }

        public bool IsPreferredLabel { get; }

        public bool IsInitialKey { get; }

        public bool IsKey { get; }

        public bool Ignore { get; }
    }
}
