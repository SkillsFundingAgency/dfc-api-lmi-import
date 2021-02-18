using System;

namespace DFC.Api.Lmi.Import.Utilities
{
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
    }
}
