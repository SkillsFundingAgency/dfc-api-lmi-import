using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using DFC.Api.Lmi.Import.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Services
{
    [Trait("Category", "LMI import service Unit Tests")]
    public class LmiImportServiceTests
    {
        private readonly ILogger<LmiImportService> fakeLogger = A.Fake<ILogger<LmiImportService>>();
        private readonly IMapLmiToGraphService fakeMapLmiToGraphService = A.Fake<IMapLmiToGraphService>();
        private readonly IJobProfileService fakeJobProfileService = A.Fake<IJobProfileService>();
        private readonly ILmiSocImportService fakeLmiSocImportService = A.Fake<ILmiSocImportService>();
        private readonly IGraphService fakeGraphService = A.Fake<IGraphService>();
        private readonly LmiImportService lmiImportService;

        public LmiImportServiceTests()
        {
            lmiImportService = new LmiImportService(fakeLogger, fakeMapLmiToGraphService, fakeJobProfileService, fakeLmiSocImportService, fakeGraphService);
        }

        [Fact]
        public async Task LmiImportServiceImportReturnsSuccess()
        {
            // arrange
            const int jobProfileMappingsCount = 2;
            var dummySocJobProfileMappings = A.CollectionOfDummy<SocJobProfileMappingModel>(jobProfileMappingsCount);
            dummySocJobProfileMappings.ToList().ForEach(f => f.Soc = 1234);

            A.CallTo(() => fakeJobProfileService.GetMappingsAsync()).Returns(dummySocJobProfileMappings);
            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).Returns(A.Dummy<LmiSocDatasetModel>());
            A.CallTo(() => fakeMapLmiToGraphService.Map(A<LmiSocDatasetModel>.Ignored)).Returns(A.Dummy<GraphSocDatasetModel>());
            A.CallTo(() => fakeGraphService.ImportAsync(A<GraphSocDatasetModel>.Ignored)).Returns(true);

            // act
            await lmiImportService.ImportAsync().ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeJobProfileService.GetMappingsAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeGraphService.PurgeAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).MustHaveHappened(jobProfileMappingsCount, Times.Exactly);
            A.CallTo(() => fakeMapLmiToGraphService.Map(A<LmiSocDatasetModel>.Ignored)).MustHaveHappened(jobProfileMappingsCount, Times.Exactly);
            A.CallTo(() => fakeGraphService.ImportAsync(A<GraphSocDatasetModel>.Ignored)).MustHaveHappened(jobProfileMappingsCount, Times.Exactly);
            Assert.True(true);
        }

        [Fact]
        public async Task LmiImportServiceImportItemAsyncReturnsSuccess()
        {
            // arrange
            const bool expectedResult = true;

            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).Returns(A.Dummy<LmiSocDatasetModel>());
            A.CallTo(() => fakeMapLmiToGraphService.Map(A<LmiSocDatasetModel>.Ignored)).Returns(A.Dummy<GraphSocDatasetModel>());
            A.CallTo(() => fakeGraphService.ImportAsync(A<GraphSocDatasetModel>.Ignored)).Returns(expectedResult);

            // act
            var result = await lmiImportService.ImportItemAsync(1234, A.CollectionOfDummy<SocJobProfileItemModel>(2).ToList()).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapLmiToGraphService.Map(A<LmiSocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeGraphService.ImportAsync(A<GraphSocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportServiceImportItemAsyncReturnsFailure()
        {
            // arrange
            const bool expectedResult = false;

            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).Returns(A.Dummy<LmiSocDatasetModel>());
            A.CallTo(() => fakeMapLmiToGraphService.Map(A<LmiSocDatasetModel>.Ignored)).Returns(A.Dummy<GraphSocDatasetModel>());
            A.CallTo(() => fakeGraphService.ImportAsync(A<GraphSocDatasetModel>.Ignored)).Returns(expectedResult);

            // act
            var result = await lmiImportService.ImportItemAsync(1234, A.CollectionOfDummy<SocJobProfileItemModel>(2).ToList()).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapLmiToGraphService.Map(A<LmiSocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeGraphService.ImportAsync(A<GraphSocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }
    }
}
