using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.Api.Lmi.Import.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Services
{
    [Trait("Category", "Graph service Unit Tests")]
    public class GraphServiceTests
    {
        private readonly ILogger<GraphService> fakeLogger = A.Fake<ILogger<GraphService>>();
        private readonly IGraphConnector fakeGraphConnector = A.Fake<IGraphConnector>();
        private readonly GraphService graphService;

        public GraphServiceTests()
        {
            graphService = new GraphService(fakeLogger, fakeGraphConnector);
        }

        [Fact]
        public async Task GraphServiceImportReturnsSuccess()
        {
            // arrange
            const bool expectedResult = true;
            var graphSocDataset = A.Dummy<GraphSocDatasetModel>();

            A.CallTo(() => fakeGraphConnector.BuildImportCommands(A<GraphSocDatasetModel>.Ignored)).Returns(A.CollectionOfDummy<string>(2));

            // act
            var result = await graphService.ImportAsync(graphSocDataset, GraphReplicaSet.Draft).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGraphConnector.BuildImportCommands(A<GraphSocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeGraphConnector.RunAsync(A<IList<string>>.Ignored, A<GraphReplicaSet>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GraphServiceImportReturnsExceptionForNullGraphSocDataset()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await graphService.ImportAsync(null, GraphReplicaSet.Draft).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGraphConnector.BuildImportCommands(A<GraphSocDatasetModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeGraphConnector.RunAsync(A<IList<string>>.Ignored, A<GraphReplicaSet>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'graphSocDataset')", exceptionResult.Message);
        }

        [Fact]
        public async Task GraphServiceImportReturnsCatchesException()
        {
            // arrange
            const bool expectedResult = false;
            var graphSocDataset = A.Dummy<GraphSocDatasetModel>();

            A.CallTo(() => fakeGraphConnector.RunAsync(A<IList<string>>.Ignored, A<GraphReplicaSet>.Ignored)).ThrowsAsync(new Exception());

            // act
            var result = await graphService.ImportAsync(graphSocDataset, GraphReplicaSet.Draft).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGraphConnector.BuildImportCommands(A<GraphSocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeGraphConnector.RunAsync(A<IList<string>>.Ignored, A<GraphReplicaSet>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GraphServicePurgeReturnsSuccess()
        {
            // arrange
            A.CallTo(() => fakeGraphConnector.BuildPurgeCommands()).Returns(A.CollectionOfDummy<string>(2));

            // act
            await graphService.PurgeAsync(GraphReplicaSet.Draft).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGraphConnector.BuildPurgeCommands()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeGraphConnector.RunAsync(A<IList<string>>.Ignored, A<GraphReplicaSet>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.True(true);
        }

        [Fact]
        public async Task GraphServicePurgeSocAsyncReturnsSuccess()
        {
            // arrange
            A.CallTo(() => fakeGraphConnector.BuildPurgeCommandsForInitialKey(A<string>.Ignored)).Returns(A.CollectionOfDummy<string>(2));

            // act
            await graphService.PurgeSocAsync(1234, GraphReplicaSet.Draft).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGraphConnector.BuildPurgeCommandsForInitialKey(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeGraphConnector.RunAsync(A<IList<string>>.Ignored, A<GraphReplicaSet>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.True(true);
        }
    }
}
