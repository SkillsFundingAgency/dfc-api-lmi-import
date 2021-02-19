using DFC.Api.Lmi.Import.Attributes;
using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DFC.Api.Lmi.Import.Services
{
    public class CypherQueryBuilderService : ICypherQueryBuilderService
    {
        private readonly GraphOptions graphOptions;

        public CypherQueryBuilderService(
            GraphOptions graphOptions)
        {
            this.graphOptions = graphOptions;
        }

        public IList<string> BuildPurgeCommands()
        {
            var commands = new List<string>();

            var graphNodeClasses = Utilities.AttributeUtilies.GetTypesWithAttribute(Assembly.GetExecutingAssembly(), typeof(GraphNodeAttribute));

            if (graphNodeClasses != null && graphNodeClasses.Any())
            {
                foreach (var graphNodeClass in graphNodeClasses)
                {
                    var graphNodeAttribute = Utilities.AttributeUtilies.GetAttribute<GraphNodeAttribute>(graphNodeClass);

                    if (graphNodeAttribute != null)
                    {
                        commands.Add(BuildPurgeCommand(graphNodeAttribute.Name));
                    }
                }
            }

            return commands;
        }

        public string BuildPurgeCommand(string nodeName)
        {
            return $"MATCH (s:{nodeName}) DETACH DELETE s";
        }

        public string BuildMerge(object item, string nodeName)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));

            const string nodeAlias = "a";
            var mergeCommand = BuildMerge(nodeAlias, nodeName, BuildKeyProperties(item));
            var setProperties = BuildSetProperties(nodeAlias, nodeName, item);

            if (!string.IsNullOrWhiteSpace(setProperties))
            {
                mergeCommand += " SET " + setProperties;
            }

            return mergeCommand;
        }

        public string BuildMerge(string nodeAlias, string nodeName, string? keyValues)
        {
            return $"MERGE ({nodeAlias}:{nodeName} {{{keyValues}}})";
        }

        public string BuildMatch(string nodeAlias, string nodeName, string? keyValues)
        {
            return $"MATCH ({nodeAlias}:{nodeName} {{{keyValues}}})";
        }

        public string BuildSetProperties<TModel>(string nodeAlias, string nodeName, TModel item)
            where TModel : class
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));

            var sb = new StringBuilder();
            var type = item.GetType();

            sb.Append(BuildSetUriProperty(nodeAlias, nodeName) + ",");

            foreach (var propertyInfo in type.GetProperties())
            {
                var graphPropertyAttribute = propertyInfo.GetCustomAttributes(typeof(GraphPropertyAttribute), false).FirstOrDefault() as GraphPropertyAttribute;
                if (graphPropertyAttribute != null && !graphPropertyAttribute.Ignore)
                {
                    if (!graphPropertyAttribute.IsKey)
                    {
                        sb.Append(BuildSetProperty(nodeAlias, graphPropertyAttribute.Name, GetPropertyValue(item, propertyInfo)) + ",");
                    }

                    if (graphPropertyAttribute.IsPreferredLabel)
                    {
                        sb.Append(BuildSetProperty(nodeAlias, graphOptions.PreferredLabelName, GetPropertyValue(item, propertyInfo)) + ",");
                    }
                }
            }

            var result = sb.ToString();

            if (result.EndsWith(",", StringComparison.OrdinalIgnoreCase))
            {
                result = result[0..^1];
            }

            return result;
        }

        public string BuildSetUriProperty(string nodeAlias, string nodeName)
        {
            return BuildSetProperty(nodeAlias, graphOptions.UriPropertyName, QuoteString($"{graphOptions.ContentApiUriPrefix}{nodeName}/{Guid.NewGuid()}".ToLowerInvariant()));
        }

        public string BuildSetProperty(string nodeAlias, string name, string? value)
        {
            return $"{nodeAlias}.{name} = {value}";
        }

        public IList<string> BuildRelationships(object parent, string parentNodeName)
        {
            _ = parent ?? throw new ArgumentNullException(nameof(parent));

            var commands = new List<string>();

            foreach (var propertyInfo in parent.GetType().GetProperties())
            {
                commands.AddRange(BuildEqualRelationship(propertyInfo, parent, parentNodeName));
                commands.AddRange(BuildChildRelationship(propertyInfo, parent, parentNodeName));
            }

            return commands;
        }

        public IList<string> BuildEqualRelationship(PropertyInfo? propertyInfo, object parent, string parentNodeName)
        {
            _ = parent ?? throw new ArgumentNullException(nameof(parent));

            var commands = new List<string>();

            var graphRelationshipRootAttribute = propertyInfo.GetCustomAttributes(typeof(GraphRelationshipRootAttribute), false).FirstOrDefault() as GraphRelationshipRootAttribute;
            if (graphRelationshipRootAttribute != null && !graphRelationshipRootAttribute.Ignore && !string.IsNullOrWhiteSpace(graphRelationshipRootAttribute.Name))
            {
                var child = propertyInfo.GetValue(parent, null);

                if (child != null)
                {
                    commands.AddRange(BuildChildRelationship(parent, child, parentNodeName, graphRelationshipRootAttribute.Name));
                }
            }

            return commands;
        }

        public IList<string> BuildChildRelationship(PropertyInfo? propertyInfo, object parent, string parentNodeName)
        {
            _ = parent ?? throw new ArgumentNullException(nameof(parent));

            var commands = new List<string>();

            var graphRelationshipAttribute = propertyInfo.GetCustomAttributes(typeof(GraphRelationshipAttribute), false).FirstOrDefault() as GraphRelationshipAttribute;
            if (graphRelationshipAttribute != null && !graphRelationshipAttribute.Ignore && !string.IsNullOrWhiteSpace(graphRelationshipAttribute.Name))
            {
                var children = propertyInfo.GetValue(parent, null) as IEnumerable<object>;
                if (children != null && children.Any())
                {
                    foreach (var child in children)
                    {
                        commands.AddRange(BuildChildRelationship(parent, child, parentNodeName, graphRelationshipAttribute.Name));
                    }
                }
            }

            return commands;
        }

        public IList<string> BuildChildRelationship(object parent, object child, string parentNodeName, string relationshipName)
        {
            _ = parent ?? throw new ArgumentNullException(nameof(parent));
            _ = child ?? throw new ArgumentNullException(nameof(child));

            var commands = new List<string>();
            var childGraphNodeAttribute = Utilities.AttributeUtilies.GetAttribute<GraphNodeAttribute>(child.GetType());

            if (childGraphNodeAttribute != null)
            {
                commands.Add(BuildMerge(child, childGraphNodeAttribute.Name));
                commands.Add(BuildRelationship(parentNodeName, childGraphNodeAttribute.Name, relationshipName, parent, child));

                commands.AddRange(BuildRelationships(child, childGraphNodeAttribute.Name));
            }

            return commands;
        }

        public string BuildRelationship(string parentNode, string childNode, string relationshipName, object parent, object child)
        {
            _ = parent ?? throw new ArgumentNullException(nameof(parent));
            _ = child ?? throw new ArgumentNullException(nameof(child));

            const char space = ' ';
            const string parentAlias = "p";
            const string childAlias = "c";
            var sb = new StringBuilder();

            sb.Append(BuildMatch(parentAlias, parentNode, BuildKeyProperties(parent)));
            sb.Append(space);
            sb.Append(BuildMatch(childAlias, childNode, BuildKeyProperties(child)));
            sb.Append(space);
            sb.Append(BuildRelationship(parentAlias, childAlias, relationshipName));

            return sb.ToString();
        }

        public string BuildRelationship(string fromAlias, string toAlias, string relationship)
        {
            return $"MERGE ({fromAlias})-[rel:{relationship}]->({toAlias})";
        }

        public string BuildKeyProperties<TModel>(TModel item)
            where TModel : class
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));

            var sb = new StringBuilder();
            var type = item.GetType();

            foreach (var propertyInfo in type.GetProperties())
            {
                var graphPropertyAttribute = propertyInfo.GetCustomAttributes(typeof(GraphPropertyAttribute), false).FirstOrDefault() as GraphPropertyAttribute;
                if (graphPropertyAttribute != null && graphPropertyAttribute.IsKey && !graphPropertyAttribute.Ignore)
                {
                    sb.Append(BuildKeyProperty(graphPropertyAttribute.Name, GetPropertyValue(item, propertyInfo)) + ",");
                }
            }

            var result = sb.ToString();

            if (result.EndsWith(",", StringComparison.OrdinalIgnoreCase))
            {
                result = result[0..^1];
            }

            return result;
        }

        public string BuildKeyProperty(string name, string? value)
        {
            return $"{name}: {value}";
        }

        public string GetPropertyValue<TModel>(TModel item, PropertyInfo propertyInfo)
            where TModel : class
        {
            _ = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));

            var value = propertyInfo.GetValue(item, null);

            switch (Type.GetTypeCode(propertyInfo.PropertyType))
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return $"{value ?? 0}";
                case TypeCode.DateTime:
                    var dt = value as DateTime?;
                    return dt.HasValue ? $"datetime('{dt:O}')" : string.Empty;
                default:
                    return QuoteString($"{value ?? string.Empty}");
            }
        }

        public string QuoteString(string? value)
        {
            const string SingleQuote = "'";
            const string TwoSingleQuotes = SingleQuote + SingleQuote;

            if (string.IsNullOrWhiteSpace(value))
            {
                return TwoSingleQuotes;
            }
            else
            {
                return SingleQuote + value.Replace(SingleQuote, TwoSingleQuotes) + SingleQuote;
            }
        }
    }
}
