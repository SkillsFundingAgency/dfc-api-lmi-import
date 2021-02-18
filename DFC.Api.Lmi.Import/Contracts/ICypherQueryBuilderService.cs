using System.Collections.Generic;
using System.Reflection;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ICypherQueryBuilderService
    {
        string BuildMerge(object item, string nodeName);

        string BuildMerge(string nodeAlias, string nodeName, string? keyValues);

        string BuildMatch(string nodeAlias, string nodeName, string? keyValues);

        string BuldSetProperties<TModel>(string nodeAlias, string nodeName, TModel item)
            where TModel : class;

        string BuildSetUriProperty(string nodeAlias, string nodeName);

        IList<string> BuildRelationships(object parent, string parentNodeName);

        IList<string> BuildChildRelationship(object parent, object child, string parentNodeName, string relationshipName);

        string BuildRelationship(string parentNode, string childNode, string relationshipName, object parent, object child);

        string BuildRelationship(string fromAlias, string toAlias, string relationship);

        string BuildKeyProperties<TModel>(TModel item)
            where TModel : class;

        string GetPropertyValue<TModel>(TModel item, PropertyInfo propertyInfo)
            where TModel : class;

        string QuoteString(string? value);

        string BuildSetProperty(string nodeAlias, string name, string? value);
    }
}