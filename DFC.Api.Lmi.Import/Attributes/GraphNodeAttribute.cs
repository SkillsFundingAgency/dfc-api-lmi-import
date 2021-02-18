using System;

namespace DFC.Api.Lmi.Import.Attributes
{
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
