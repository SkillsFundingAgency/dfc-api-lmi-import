using DFC.Api.Lmi.Import.Connectors;
using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Connectors
{
    [Trait("Category", "Graph connector Unit Tests")]
    public class GraphConnectorTests
    {
        private readonly IGraphCluster fakeGraphCluster = A.Fake<IGraphCluster>();
        private readonly IServiceProvider fakeServiceProvider = A.Fake<IServiceProvider>();
        private readonly GraphOptions graphOptions = new GraphOptions();
        private readonly ICypherQueryBuilderService fakeCypherQueryBuilderService = A.Fake<ICypherQueryBuilderService>();
        private readonly GraphConnector graphConnector;

        public GraphConnectorTests()
        {
            graphConnector = new GraphConnector(fakeGraphCluster, fakeServiceProvider, graphOptions, fakeCypherQueryBuilderService);
        }

        [Fact]
        public void GraphConnectorBuildPurgeCommandsReturnsSuccess()
        {
            // arrange
            var expectedResults = new List<string>
            {
                "string one",
                "string two",
            };

            A.CallTo(() => fakeCypherQueryBuilderService.BuildPurgeCommands()).Returns(expectedResults);

            // act
            var results = graphConnector.BuildPurgeCommands();

            // assert
            A.CallTo(() => fakeCypherQueryBuilderService.BuildPurgeCommands()).MustHaveHappenedOnceExactly();
            Assert.NotNull(results);
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public void GraphConnectorBuildPurgeCommandsForInitialKeyReturnsSuccess()
        {
            // arrange
            var expectedResults = new List<string>
            {
                "string one",
                "string two",
            };

            A.CallTo(() => fakeCypherQueryBuilderService.BuildPurgeCommandsForInitialKey(A<string>.Ignored)).Returns(expectedResults);

            // act
            var results = graphConnector.BuildPurgeCommandsForInitialKey("a-key");

            // assert
            A.CallTo(() => fakeCypherQueryBuilderService.BuildPurgeCommandsForInitialKey(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.NotNull(results);
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public void GraphConnectorBuildImportCommandsReturnsSuccess()
        {
            // arrange
            var someStrings = new List<string>
            {
                "string one",
                "string two",
            };
            var graphSocDataset = new GraphSocDatasetModel();

            A.CallTo(() => fakeCypherQueryBuilderService.BuildMerge(A<GraphBaseSocModel>.Ignored, A<string>.Ignored)).Returns("a string");
            A.CallTo(() => fakeCypherQueryBuilderService.BuildRelationships(A<GraphBaseSocModel>.Ignored, A<string>.Ignored)).Returns(someStrings);

            // act
            var results = graphConnector.BuildImportCommands(graphSocDataset);

            // assert
            A.CallTo(() => fakeCypherQueryBuilderService.BuildMerge(A<GraphBaseSocModel>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeCypherQueryBuilderService.BuildRelationships(A<GraphBaseSocModel>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.True(results.Count >= 1);
        }

        [Fact]
        public void GraphConnectorBuildImportCommandsReturnsExceptionForNullParent()
        {
            // arrange
            GraphSocDatasetModel? graphSocDataset = default;

            // act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => graphConnector.BuildImportCommands(graphSocDataset));

            // assert
            A.CallTo(() => fakeCypherQueryBuilderService.BuildMerge(A<GraphBaseSocModel>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeCypherQueryBuilderService.BuildRelationships(A<GraphBaseSocModel>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'parent')", exceptionResult.Message);
        }

        [Theory]
        [InlineData(GraphReplicaSet.Published)]
        [InlineData(GraphReplicaSet.Draft)]
        public async Task GraphConnectorRunReturnsSuccess(GraphReplicaSet graphReplicaSet)
        {
            // arrange
            var commands = new List<string>
            {
                "command one",
                "command two",
            };

            A.CallTo(() => fakeServiceProvider.GetService(typeof(ICustomCommand))).Returns(new CustomCommand());

            // act
            await graphConnector.RunAsync(commands, graphReplicaSet).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeServiceProvider.GetService(typeof(ICustomCommand))).MustHaveHappened(commands.Count, Times.Exactly);
            A.CallTo(() => fakeGraphCluster.Run(A<string>.Ignored, A<ICommand[]>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.True(true);
        }

        [Fact]
        public async Task GraphConnectorRunReturnsExceptionForNullCommands()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await graphConnector.RunAsync(null, GraphReplicaSet.Draft).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeServiceProvider.GetService(typeof(ICustomCommand))).MustNotHaveHappened();
            A.CallTo(() => fakeGraphCluster.Run(A<string>.Ignored, A<ICommand[]>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'commands')", exceptionResult.Message);
        }

        [Fact]
        public async Task GraphConnectorRunReturnsExceptionForBadGraphReplicaSet()
        {
            // arrange
            var commands = new List<string>
            {
                "command one",
                "command two",
            };

            // act
            var exceptionResult = await Assert.ThrowsAsync<NotImplementedException>(async () => await graphConnector.RunAsync(commands, GraphReplicaSet.None).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeServiceProvider.GetService(typeof(ICustomCommand))).MustNotHaveHappened();
            A.CallTo(() => fakeGraphCluster.Run(A<string>.Ignored, A<ICommand[]>.Ignored)).MustNotHaveHappened();
        }
    }
}
