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
        private const string HttpContentApi = "https://content.api.com/";
        private readonly CypherQueryBuilderService cypherQueryBuilderService;
        private readonly IList<string> definedGraphNodeNames = new List<string>
        {
            "LmiSocBreakdown",
            "LmiSocBreakdownYear",
            "LmiSocBreakdownYearValue",
            "LmiSocJobProfile",
            "LmiSocPredicted",
            "LmiSocPredictedYear",
            "LmiSoc",
        };

        public CypherQueryBuilderServiceTests()
        {
            var graphOptions = new GraphOptions
            {
                ContentApiUriPrefix = new Uri(HttpContentApi, UriKind.Absolute),
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
            const string nodeName = "NodeName";
            var graphJobProfile = new GraphJobProfileModel
            {
                CanonicalName = "canonical-name",
                Title = "The title",
            };
            string expectedResult = $"MERGE (a:{nodeName} {{CanonicalName: '{graphJobProfile.CanonicalName}'}}) SET a.uri = '{HttpContentApi}{nodeName.ToLowerInvariant()}/{graphJobProfile.ItemId.ToString().ToLowerInvariant()}',a.skos__prefLabel = '{graphJobProfile.CanonicalName}',a.Title = '{graphJobProfile.Title}',a.CreatedDate = datetime('{graphJobProfile.CreatedDate:O}')";

            //act
            var result = cypherQueryBuilderService.BuildMerge(graphJobProfile, nodeName);

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildMergeReturnsExceptionForNullItem()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildMerge(null, string.Empty));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'item')", exceptionResult.Message);
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
            const string nodeAlias = "n";
            const string nodeName = "NodeName";
            const int soc = 3231;
            const int year = 2021;
            var item = new GraphPredictedYearModel
            {
                Soc = soc,
                Year = year,
                Measure = "a measure",
                Employment = new decimal(1234.5678),
            };
            var expectedResult = $"{nodeAlias}.uri = '{HttpContentApi}{nodeName.ToLowerInvariant()}/{item.ItemId.ToString().ToLowerInvariant()}',{nodeAlias}.Measure = '{item.Measure}',{nodeAlias}.skos__prefLabel = {item.Year},{nodeAlias}.Employment = {item.Employment},{nodeAlias}.CreatedDate = datetime('{item.CreatedDate:O}')";

            //act
            var result = cypherQueryBuilderService.BuildSetProperties(nodeAlias, nodeName, item);

            //assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildSetPropertiesReturnsExceptionForNullItem()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildSetProperties(string.Empty, string.Empty, null));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'item')", exceptionResult.Message);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildSetUriPropertyReturnsSuccess()
        {
            //arrange
            Guid itemId = Guid.NewGuid();
            const string nodeAlias = "n";
            const string nodeName = "NodeName";
            string expectedResult = $"{nodeAlias}.uri = '{HttpContentApi}{nodeName}/{itemId}'".ToLowerInvariant();

            //act
            var result = cypherQueryBuilderService.BuildSetUriProperty(nodeAlias, nodeName, itemId);

            //assert
            Assert.Equal(expectedResult, result);
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
        public void CypherQueryBuilderServiceBuildRelationshipsReturnsExceptionForNullParent()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildRelationships(null, string.Empty));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'parent')", exceptionResult.Message);
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
            var expectedResults = new List<string>
            {
                $"MERGE (a:LmiSocPredicted {{Soc: {soc}}}) SET a.uri = '{HttpContentApi}lmisocpredicted/{item.JobGrowth.ItemId.ToString().ToLowerInvariant()}',a.Measure = '{item.JobGrowth.Measure}',a.skos__prefLabel = '{item.JobGrowth.Measure}',a.CreatedDate = datetime('{item.JobGrowth.CreatedDate:O}')",
                $"MATCH (p:{nodeName} {{Soc: {soc}}}) MATCH (c:LmiSocPredicted {{Soc: {soc}}}) MERGE (p)-[rel:PredictedJobGrowth]->(c)",
            };

            //act
            var results = cypherQueryBuilderService.BuildEqualRelationship(propertyInfo, item, nodeName);

            //assert
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildEqualRelationshipReturnsExceptionForNullPropertyInfo()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildEqualRelationship(null, new GraphSocDatasetModel(), string.Empty));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'propertyInfo')", exceptionResult.Message);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildEqualRelationshipReturnsExceptionForNullParent()
        {
            //arrange
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

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildEqualRelationship(propertyInfo, null, string.Empty));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'parent')", exceptionResult.Message);
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
            var expectedResults = new List<string>
            {
                $"MERGE (a:LmiSocPredictedYear {{Year: {year},Soc: {soc}}}) SET a.uri = '{HttpContentApi}lmisocpredictedyear/{item.PredictedEmployment.First().ItemId.ToString().ToLowerInvariant()}',a.Measure = '{item.PredictedEmployment.First().Measure:O}',a.skos__prefLabel = {year},a.Employment = 1234.5678,a.CreatedDate = datetime('{item.PredictedEmployment.First().CreatedDate:O}')",
                $"MATCH (p:{nodeName} {{Soc: {soc}}}) MATCH (c:LmiSocPredictedYear {{Year: {year},Soc: {soc}}}) MERGE (p)-[rel:PredictedEmployment]->(c)",
            };

            //act
            var results = cypherQueryBuilderService.BuildChildRelationship(propertyInfo, item, nodeName);

            //assert
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildChildRelationshipReturnsExceptionForNullPropertyInfo()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildChildRelationship(null, new GraphSocDatasetModel(), string.Empty));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'propertyInfo')", exceptionResult.Message);
        }

        [Fact]
        public void CypherQueryBuilderServiceBuildChildRelationshipReturnsExceptionForNullParent()
        {
            //arrange
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

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildChildRelationship(propertyInfo, null, string.Empty));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'parent')", exceptionResult.Message);
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
            var expectedResults = new List<string>
            {
                $"MERGE (a:LmiSocPredictedYear {{Year: {year},Soc: {soc}}}) SET a.uri = '{HttpContentApi}lmisocpredictedyear/{child.ItemId.ToString().ToLowerInvariant()}',a.Measure = '',a.skos__prefLabel = 2021,a.Employment = 0,a.CreatedDate = datetime('{child.CreatedDate:O}')",
                $"MATCH (p:{nodeName} {{Soc: {soc}}}) MATCH (c:LmiSocPredictedYear {{Year: {year},Soc: {soc}}}) MERGE (p)-[rel:{relName}]->(c)",
            };

            //act
            var results = cypherQueryBuilderService.BuildChildRelationship(parent, child, nodeName, relName);

            //assert
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public void CypherQueryBuilderServicBuildChildRelationshipReturnsExceptionForNullParent()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildChildRelationship(null, new GraphPredictedYearModel(), string.Empty, string.Empty));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'parent')", exceptionResult.Message);
        }

        [Fact]
        public void CypherQueryBuilderServicBuildChildRelationshipReturnsExceptionForNullChild()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildChildRelationship(new GraphPredictedModel(), null, string.Empty, string.Empty));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'child')", exceptionResult.Message);
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
        public void CypherQueryBuilderServicBuildRelationshipReturnsExceptionForNullParent()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildRelationship(string.Empty, string.Empty, string.Empty, null, new GraphPredictedYearModel()));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'parent')", exceptionResult.Message);
        }

        [Fact]
        public void CypherQueryBuilderServicBuildRelationshipReturnsExceptionForNullChild()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildRelationship(string.Empty, string.Empty, string.Empty, new GraphPredictedModel(), null));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'child')", exceptionResult.Message);
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
            var item = new GraphBreakdownYearValueModel
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
        public void CypherQueryBuilderServicBuildKeyPropertiesReturnsExceptionForNullItem()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.BuildKeyProperties(null));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'item')", exceptionResult.Message);
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

        [Fact]
        public void CypherQueryBuilderServicGetPropertyValueReturnsExceptionForNullItem()
        {
            //arrange
            var item = new GraphPredictedYearModel();
            var propertyInfo = item.GetType().GetProperty(nameof(GraphPredictedYearModel.Soc));

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.GetPropertyValue(null, propertyInfo));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'item')", exceptionResult.Message);
        }

        [Fact]
        public void CypherQueryBuilderServicBuildKeyPropertiesReturnsExceptionForNullPropertyInfo()
        {
            //arrange

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => cypherQueryBuilderService.GetPropertyValue(new GraphPredictedYearModel(), null));

            //assert
            Assert.Equal("Value cannot be null. (Parameter 'propertyInfo')", exceptionResult.Message);
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
