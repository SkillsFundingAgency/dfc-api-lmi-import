using DFC.Api.Lmi.Import.Attributes;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.Api.Lmi.Import.Services;
using DFC.Api.Lmi.Import.Startup;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Services
{
    [Trait("Category", "Cypher query builder service Unit Tests")]
    public class CypherQueryBuilderServiceTests
    {
        private readonly CypherQueryBuilderService cypherQueryBuilderService;
        private readonly IList<string> definedGraphNodeNames = new List<string>
        {
            "LmiSocBreakdown",
            "LmiSocBreakdownYearItem",
            "LmiSocBreakdownYear",
            "LmiSocJobProfile",
            "LmiSocPredicted",
            "LmiSocPredictedYear",
            "LmiSoc",
        };

        public CypherQueryBuilderServiceTests()
        {
            var graphOptions = new GraphOptions
            {
                ContentApiUriPrefix = new Uri("https://content.api.com/", UriKind.Absolute),
            };
            cypherQueryBuilderService = new CypherQueryBuilderService(graphOptions);
        }

        [Fact]
        public void CypherQueryBuilderServiceCheckGraphModelIntegrityReturnsSuccess()
        {
            foreach (var graphNodeType in Utilities.AttributeUtilies.GetTypesWithAttribute(Assembly.GetAssembly(typeof(WebJobsExtensionStartup)), typeof(GraphNodeAttribute)))
            {
                //arrange
                int keyCount = 0;
                int preferredLabelCount = 0;

                //act
                foreach (var propertyInfo in graphNodeType!.GetProperties())
                {
                    var graphPropertyAttribute = propertyInfo.GetCustomAttributes(typeof(GraphPropertyAttribute), false).FirstOrDefault() as GraphPropertyAttribute;
                    if (graphPropertyAttribute != null && graphPropertyAttribute.IsKey && !graphPropertyAttribute.Ignore)
                    {
                        keyCount++;
                    }

                    if (graphPropertyAttribute != null && graphPropertyAttribute.IsPreferredLabel && !graphPropertyAttribute.Ignore)
                    {
                        preferredLabelCount++;
                    }
                }

                //assert
                Assert.NotEqual(0, keyCount);
                Assert.Equal(1, preferredLabelCount);
            }
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildPurgeCommandsReturnsSuccess()
        {
            //arrange
            var expectedResults = (from a in definedGraphNodeNames select $"MATCH (s:{a}) DETACH DELETE s").ToList();

            //act
            var results = cypherQueryBuilderService.BuildPurgeCommands();

            //assert
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildPurgeCommandReturnsSuccess()
        {
            //arrange
            const string expectedResult = "MATCH (s:NodeName) DETACH DELETE s";

            //act
            var result = cypherQueryBuilderService.BuildPurgeCommand("NodeName");

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildMergeReturnsSuccess()
        {
            //arrange
            var graphJobProfile = new GraphJobProfileModel
            {
                CanonicalName = "canonical-name",
                Title = "The title",
            };
            string expectedResultStartPart = $"MERGE (a:NodeName {{CanonicalName: '{graphJobProfile.CanonicalName}'}}) SET a.uri = '";
            string expectedResultEndPart = $"',a.skos__prefLabel = '{graphJobProfile.CanonicalName}',a.Title = '{graphJobProfile.Title}'";

            //act
            var result = cypherQueryBuilderService.BuildMerge(graphJobProfile, "NodeName");

            //assert
            Assert.StartsWith(expectedResultStartPart, result);
            Assert.EndsWith(expectedResultEndPart, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildMergePartReturnsSuccess()
        {
            //arrange
            const string keyValues = "aString:\"a string\",aNumber:123";
            const string expectedResult = "MERGE (n:NodeName {" + keyValues + "})";

            //act
            var result = cypherQueryBuilderService.BuildMerge("n", "NodeName", keyValues);

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildMatchReturnsSuccess()
        {
            //arrange
            const string keyValues = "aString:\"a string\",aNumber:123";
            const string expectedResult = "MATCH (n:NodeName {" + keyValues + "})";

            //act
            var result = cypherQueryBuilderService.BuildMatch("n", "NodeName", keyValues);

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildSetPropertiesReturnsSuccess()
        {
            //arrange
            const int soc = 3231;
            const int year = 2021;
            var item = new GraphPredictedYearModel
            {
                Soc = soc,
                Year = year,
                CreatedDate = DateTime.UtcNow,
                Measure = "a measure",
                Employment = new decimal(1234.5678),
            };
            var expectedResultStartsWith = "a.uri = '";
            var expectedResultEndsWith = $",a.Measure = '{item.Measure}',a.skos__prefLabel = {item.Year},a.Employment = {item.Employment},a.CreatedDate = datetime('{item.CreatedDate:O}')";

            //act
            var result = cypherQueryBuilderService.BuildSetProperties("a", "nodeName", item);

            //assert
            Assert.StartsWith(expectedResultStartsWith, result);
            Assert.EndsWith(expectedResultEndsWith, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildSetUriPropertyReturnsSuccess()
        {
            //arrange
            const string expectedResultStartPart = "n.uri = '";
            const string expectedResultEndPart = "'";

            //act
            var result = cypherQueryBuilderService.BuildSetUriProperty("n", "NodeName");

            //assert
            Assert.StartsWith(expectedResultStartPart, result);
            Assert.EndsWith(expectedResultEndPart, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildSetPropertyReturnsSuccess()
        {
            //arrange
            const string expectedResult = "n.NodeName = value";

            //act
            var result = cypherQueryBuilderService.BuildSetProperty("n", "NodeName", "value");

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildRelationshipsReturnsSuccess()
        {
            //arrange
            const string nodeName = "nodeName";
            const int soc = 3231;
            var item = new GraphSocDatasetModel
            {
                Soc = soc,
            };

            //act
            var results = cypherQueryBuilderService.BuildRelationships(item, nodeName);

            //assert
            Assert.Equal(0, results.Count);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildEqualRelationshipReturnsSuccess()
        {
            //arrange
            const string nodeName = "nodeName";
            const int soc = 3231;
            var item = new GraphSocDatasetModel
            {
                Soc = soc,
                JobGrowth = new GraphPredictedModel
                {
                    Soc = soc,
                    Measure = "a measure",
                    CreatedDate = DateTime.UtcNow,
                },
            };
            var propertyInfo = item.GetType().GetProperty(nameof(GraphSocDatasetModel.JobGrowth));
            var expectedFirstResultStartsWith = $"MERGE (a:LmiSocPredicted {{Soc: {soc}}}) SET a.uri = '";
            var expectedFirstResultEndsWith = $"',a.Measure = '{item.JobGrowth.Measure}',a.skos__prefLabel = '{item.JobGrowth.Measure}',a.CreatedDate = datetime('{item.JobGrowth.CreatedDate:O}')";
            var expectedSecondResult = $"MATCH (p:{nodeName} {{Soc: {soc}}}) MATCH (c:LmiSocPredicted {{Soc: {soc}}}) MERGE (p)-[rel:PredictedJobGrowth]->(c)";

            //act
            var results = cypherQueryBuilderService.BuildEqualRelationship(propertyInfo, item, nodeName);

            //assert
            Assert.StartsWith(expectedFirstResultStartsWith, results[0]);
            Assert.EndsWith(expectedFirstResultEndsWith, results[0]);
            Assert.Equal(expectedSecondResult, results[1]);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildChildRelationshipReturnsSuccess()
        {
            //arrange
            const string nodeName = "nodeName";
            const int soc = 3231;
            const int year = 2021;
            var item = new GraphPredictedModel
            {
                Soc = soc,
                PredictedEmployment = new List<GraphPredictedYearModel>
                {
                    new GraphPredictedYearModel
                    {
                        Soc = soc,
                        Year = year,
                        Measure = "a measure",
                        Employment = new decimal(1234.5678),
                        CreatedDate = DateTime.UtcNow,
                    },
                },
            };
            var propertyInfo = item.GetType().GetProperty(nameof(GraphPredictedModel.PredictedEmployment));
            var expectedFirstResultStartsWith = $"MERGE (a:LmiSocPredictedYear {{Year: {year},Soc: {soc}}}) SET a.uri = '";
            var expectedFirstResultEndsWith = $"',a.Measure = '{item.PredictedEmployment.First().Measure:O}',a.skos__prefLabel = {year},a.Employment = 1234.5678,a.CreatedDate = datetime('{item.PredictedEmployment.First().CreatedDate:O}')";
            var expectedSecondResult = $"MATCH (p:{nodeName} {{Soc: {soc}}}) MATCH (c:LmiSocPredictedYear {{Year: {year},Soc: {soc}}}) MERGE (p)-[rel:PredictedEmployment]->(c)";

            //act
            var results = cypherQueryBuilderService.BuildChildRelationship(propertyInfo, item, nodeName);

            //assert
            Assert.StartsWith(expectedFirstResultStartsWith, results[0]);
            Assert.EndsWith(expectedFirstResultEndsWith, results[0]);
            Assert.Equal(expectedSecondResult, results[1]);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildChildRelationshipItemReturnsSuccess()
        {
            //arrange
            const int soc = 3231;
            const int year = 2021;
            const string nodeName = "nodeName";
            const string relName = "relNme";
            var parent = new GraphPredictedModel { Soc = soc };
            var child = new GraphPredictedYearModel { Soc = soc, Year = year };
            var expectedResultFirstPart = $"MERGE (a:LmiSocPredictedYear {{Year: {year},Soc: {soc}}}) SET a.uri = '";
            var expcetedResultSecondPart = $"MATCH (p:{nodeName} {{Soc: {soc}}}) MATCH (c:LmiSocPredictedYear {{Year: {year},Soc: {soc}}}) MERGE (p)-[rel:{relName}]->(c)";

            //act
            var results = cypherQueryBuilderService.BuildChildRelationship(parent, child, nodeName, relName);

            //assert
            Assert.StartsWith(expectedResultFirstPart, results[0]);
            Assert.Equal(expcetedResultSecondPart, results[1]);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildRelationshipReturnsSuccess()
        {
            //arrange
            const char space = ' ';
            const int soc = 3231;
            const int year = 2021;
            var parent = new GraphPredictedModel { Soc = soc };
            var child = new GraphPredictedYearModel { Soc = soc, Year = year };
            var sb = new StringBuilder();
            sb.Append($"MATCH (p:parent {{Soc: {soc}}})");
            sb.Append(space);
            sb.Append($"MATCH (c:child {{Year: {year},Soc: {soc}}})");
            sb.Append(space);
            sb.Append("MERGE (p)-[rel:relName]->(c)");
            var expectedResult = sb.ToString();

            //act
            var result = cypherQueryBuilderService.BuildRelationship("parent", "child", "relName", parent, child);

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildRelationshipItemReturnsSuccess()
        {
            //arrange
            const string expectedResult = "MERGE (f)-[rel:relName]->(t)";

            //act
            var result = cypherQueryBuilderService.BuildRelationship("f", "t", "relName");

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildKeyPropertiesReturnsSuccess()
        {
            //arrange
            var item = new GraphBreakdownYearItemModel
            {
                Soc = 3231,
                Year = 2021,
                Measure = "a measure",
                Code = 1,
            };
            var expectedResult = $"Measure: '{item.Measure}',Year: {item.Year},Code: {item.Code},Soc: {item.Soc}";

            //act
            var result = cypherQueryBuilderService.BuildKeyProperties(item);

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildKeyPropertyReturnsSuccess()
        {
            //arrange
            const string expectedResult = "name: value";

            //act
            var result = cypherQueryBuilderService.BuildKeyProperty("name", "value");

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(3231, "2021-02-19 10:55:13", "1234.5678", "a measure", nameof(GraphPredictedYearModel.Soc), "3231")]
        [InlineData(3231, "2021-02-19 10:55:13", "1234.5678", "a measure", nameof(GraphPredictedYearModel.CreatedDate), "datetime('2021-02-19T10:55:13.0000000')")]
        [InlineData(3231, "2021-02-19 10:55:13", "1234.5678", "a measure", nameof(GraphPredictedYearModel.Employment), "1234.5678")]
        [InlineData(3231, "2021-02-19 10:55:13", "1234.5678", "a measure", nameof(GraphPredictedYearModel.Measure), "'a measure'")]
        public void CypherQueryBuilderServiceGetPropertyValueReturnsSuccess(int soc, string createdDate, string employment, string measure, string fieldName, string expectedResult)
        {
            //arrange
            var item = new GraphPredictedYearModel
            {
                Soc = soc,
                CreatedDate = DateTime.Parse(createdDate, CultureInfo.InvariantCulture),
                Employment = decimal.Parse(employment, CultureInfo.InvariantCulture),
                Measure = measure,
            };
            var propertyInfo = item.GetType().GetProperty(fieldName);

            //act
            var result = cypherQueryBuilderService.GetPropertyValue(item, propertyInfo!);

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(null, "''")]
        [InlineData("", "''")]
        [InlineData("value", "'value'")]
        [InlineData("a 'quoted' value", "'a ''quoted'' value'")]
        public void CypherQueryBuilderServiceQuoteStringReturnsSuccess(string? data, string expectedResult)
        {
            //arrange

            //act
            var result = cypherQueryBuilderService.QuoteString(data);

            //assert
            Assert.Equal(expectedResult, result);
        }
    }
}
