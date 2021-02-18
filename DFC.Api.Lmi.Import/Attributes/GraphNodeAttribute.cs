using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Attributes
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphNodeAttribute : Attribute
    {
        public GraphNodeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
