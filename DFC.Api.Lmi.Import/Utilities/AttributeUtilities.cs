using DFC.Api.Lmi.Import.Attributes;
using DFC.Api.Lmi.Import.Models.GraphData;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace DFC.Api.Lmi.Import.Utilities
{
    [ExcludeFromCodeCoverage]
    public static class AttributeUtilities
    {
        public static TModel? GetAttribute<TModel>(Type type)
            where TModel : Attribute
        {
            foreach (var attr in Attribute.GetCustomAttributes(type))
            {
                if (attr is TModel tmodel)
                {
                    return tmodel;
                }
            }

            return default;
        }

        public static IEnumerable<Type> GetTypesWithAttribute(Assembly assembly, Type attributeType)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(attributeType, true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        public static string? GetGraphNodeName<TModel>()
            where TModel : GraphBaseModel
        {
            var graphNodeAttribute = GetAttribute<GraphNodeAttribute>(typeof(TModel));
            return graphNodeAttribute?.Name;
        }

        public static string? GetGraphRelationshipName<TModel>(string propertyName)
            where TModel : GraphBaseModel
        {
            var propertyInfo = typeof(TModel).GetProperties().FirstOrDefault(f => f.Name == propertyName);
            var graphRelationshipAttribute = propertyInfo.GetCustomAttributes(typeof(GraphRelationshipAttribute), false).FirstOrDefault() as GraphRelationshipAttribute;
            return graphRelationshipAttribute?.Name;
        }
    }
}
