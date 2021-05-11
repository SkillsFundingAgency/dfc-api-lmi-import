using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.Api.Lmi.Import.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Services
{
    [Trait("Category", "Soc graph query service Unit Tests")]
    public class SocGraphQueryServiceTests
    {
        private readonly ILogger<SocGraphQueryService> fakeLogger = A.Fake<ILogger<SocGraphQueryService>>();
        private readonly IGenericGraphQueryService fakeGenericGraphQueryService = A.Fake<IGenericGraphQueryService>();
        private readonly SocGraphQueryService socGraphQueryService;

        public SocGraphQueryServiceTests()
        {
            socGraphQueryService = new SocGraphQueryService(fakeLogger, fakeGenericGraphQueryService);
        }

        [Fact]
        public async Task SocGraphQueryServiceGetSummaryReturnsSuccess()
        {
            // arrange
            var expectedResults = A.CollectionOfDummy<SocModel>(2).ToList();

            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<SocModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).Returns(expectedResults);

            // act
            var results = await socGraphQueryService.GetSummaryAsync(GraphReplicaSet.Draft).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<SocModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public async Task SocGraphQueryServiceGetDetailReturnsSuccess()
        {
            // arrange
            var expectedResults = A.CollectionOfDummy<GraphSocDatasetModel>(2).ToList();
            var expectedResult = expectedResults.First();

            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphSocDatasetModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).Returns(expectedResults);

            // act
            var result = await socGraphQueryService.GetDetailAsync(GraphReplicaSet.Draft, 1234).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphSocDatasetModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task SocGraphQueryServiceGetJobProfilesReturnsSuccess()
        {
            // arrange
            var expectedResults = A.CollectionOfDummy<GraphJobProfileModel>(2).ToList();

            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphJobProfileModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).Returns(expectedResults);

            // act
            var results = await socGraphQueryService.GetJobProfilesAsync(GraphReplicaSet.Draft, 1234).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphJobProfileModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public async Task SocGraphQueryServiceGetPredictedReturnsSuccess()
        {
            // arrange
            var expectedResults = A.CollectionOfDummy<GraphPredictedModel>(2).ToList();
            var expectedResult = expectedResults.First();

            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphPredictedModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).Returns(expectedResults);

            // act
            var result = await socGraphQueryService.GetPredictedAsync(GraphReplicaSet.Draft, 1234).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphPredictedModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task SocGraphQueryServiceGetPredictedYearReturnsSuccess()
        {
            // arrange
            var expectedResults = A.CollectionOfDummy<GraphPredictedYearModel>(2).ToList();

            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphPredictedYearModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).Returns(expectedResults);

            // act
            var results = await socGraphQueryService.GetPredictedYearAsync(GraphReplicaSet.Draft, 1234).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphPredictedYearModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public async Task SocGraphQueryServiceGetBreakdownReturnsSuccess()
        {
            // arrange
            var expectedResults = A.CollectionOfDummy<GraphBreakdownModel>(2).ToList();
            var expectedResult = expectedResults.First();

            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphBreakdownModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).Returns(expectedResults);

            // act
            var result = await socGraphQueryService.GetBreakdownAsync(GraphReplicaSet.Draft, 1234, "a-relationship-name").ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphBreakdownModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task SocGraphQueryServiceGetBreakdownYearReturnsSuccess()
        {
            // arrange
            var expectedResults = A.CollectionOfDummy<GraphBreakdownYearModel>(2).ToList();

            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphBreakdownYearModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).Returns(expectedResults);

            // act
            var results = await socGraphQueryService.GetBreakdownYearAsync(GraphReplicaSet.Draft, 1234, "a-relationship-name").ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphBreakdownYearModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResults, results);
        }

        [Fact]
        public async Task SocGraphQueryServiceGetBreakdownYearValueReturnsSuccess()
        {
            // arrange
            var expectedResults = A.CollectionOfDummy<GraphBreakdownYearValueModel>(2).ToList();

            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphBreakdownYearValueModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).Returns(expectedResults);

            // act
            var results = await socGraphQueryService.GetBreakdownYearValueAsync(GraphReplicaSet.Draft, 1234, "a-relationship-name").ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeGenericGraphQueryService.ExecuteCypherQuery<GraphBreakdownYearValueModel>(A<GraphReplicaSet>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResults, results);
        }
    }
}
