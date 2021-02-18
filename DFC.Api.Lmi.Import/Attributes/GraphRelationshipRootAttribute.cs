using System;

namespace DFC.Api.Lmi.Import.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GraphRelationshipRootAttribute : Attribute
    {
        public GraphRelationshipRootAttribute(string name, bool ignore = false)
        {
            Name = name;
            Ignore = ignore;
        }

        public string Name { get; }

        public bool Ignore { get; }
    }
}
