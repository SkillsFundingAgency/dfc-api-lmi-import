using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DFC.Api.Lmi.Import.Utilities
{
    [ExcludeFromCodeCoverage]
    public static class AttributeUtilies
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
    }
}
