using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Attributes
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property)]
    public class GraphPropertyAttribute : Attribute
    {
        public GraphPropertyAttribute(string name, bool isPreferredLabel = false, bool isKey = false, bool ignore = false)
        {
            Name = name;
            IsPreferredLabel = isPreferredLabel;
            IsKey = isKey;
            Ignore = ignore;
        }

        public string Name { get; }

        public bool IsPreferredLabel { get; }

        public bool IsKey { get; }

        public bool Ignore { get; }
    }
}
