using HtmlAgilityPack;
using System;
using System.Linq;

namespace DFC.Api.Lmi.Import.Utilities
{
    public static class TextSanitizerUtilities
    {
        public static void Sanitize<TModel>(TModel? model)
           where TModel : class
        {
            _ = model ?? throw new ArgumentNullException(nameof(model));

            var type = model.GetType();

            foreach (var propertyInfo in type.GetProperties().Where(w => Type.GetTypeCode(w.PropertyType) == TypeCode.String))
            {
                var value = propertyInfo.GetValue(model, null) as string;
                value = SanitizeString(value);
                propertyInfo.SetValue(model, value);
            }
        }

        public static string? SanitizeString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var htmlDoc = new HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionCheckSyntax = true,
                OptionAutoCloseOnEnd = true,
            };
            htmlDoc.LoadHtml(value);

            const string oneSpace = " ";
            var result = HtmlEntity.DeEntitize(htmlDoc.DocumentNode.InnerText);
            result = result.Replace("\t", oneSpace).Replace(Environment.NewLine, oneSpace);

            return result;
        }
    }
}
