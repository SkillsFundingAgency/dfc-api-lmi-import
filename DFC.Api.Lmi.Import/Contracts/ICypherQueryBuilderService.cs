using DFC.Api.Lmi.Import.Models.GraphData;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ICypherQueryBuilderService
    {
        IList<string> BuildPurgeCommands();

        string BuildPurgeCommand(string nodeName);

        string BuildMerge(GraphBaseModel? item, string nodeName);

        string BuildMerge(string nodeAlias, string nodeName, string? keyValues);

        string BuildMatch(string nodeAlias, string nodeName, string? keyValues);

        string BuildSetProperties(string nodeAlias, string nodeName, GraphBaseModel item);

        string BuildSetUriProperty(string nodeAlias, string nodeName, Guid itemId);

        IList<string> BuildRelationships(GraphBaseModel? parent, string parentNodeName);

        IList<string> BuildEqualRelationship(PropertyInfo? propertyInfo, GraphBaseModel? parent, string parentNodeName);

        IList<string> BuildChildRelationship(PropertyInfo? propertyInfo, GraphBaseModel? parent, string parentNodeName);

        IList<string> BuildChildRelationship(GraphBaseModel? parent, GraphBaseModel? child, string parentNodeName, string relationshipName);

        string BuildRelationship(string parentNode, string childNode, string relationshipName, GraphBaseModel? parent, GraphBaseModel? child);

        string BuildRelationship(string fromAlias, string toAlias, string relationship);

        string BuildKeyProperties(GraphBaseModel? item);

        string GetPropertyValue(GraphBaseModel? item, PropertyInfo? propertyInfo);

        string QuoteString(string? value);

        string BuildSetProperty(string nodeAlias, string name, string? value);
    }
}