using System;

namespace DFC.Api.Lmi.Import.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphNodeAttribute : Attribute
    {
        public GraphNodeAttribute(string nodeAlias, string nodeName)
        {
            NodeAlias = nodeAlias;
            NodeName = nodeName;
        }

        public string NodeAlias { get; }

        public string NodeName { get; }
    }
}
