using DFC.Api.Lmi.Import.Models.GraphData;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ICypherQueryBuilderService
    {
        IList<string> BuildPurgeCommands();

        IList<string> BuildPurgeCommandsForInitialKey(string key);

        string BuildPurgeCommand(string nodeName);

        string BuildPurgeCommandForInitialKey(string nodeName, string? keyValues);

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

        string BuildKeyProperty(string name, string? value);

        string BuildInitialKeyProperties(Type? type, string key);

        string GetPropertyValue(GraphBaseModel? item, PropertyInfo? propertyInfo);

        string EncodeValue(PropertyInfo? propertyInfo, object? value);

        string QuoteString(string? value);

        string BuildSetProperty(string nodeAlias, string name, string? value);
    }
}