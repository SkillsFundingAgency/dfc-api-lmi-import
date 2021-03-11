using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Functions;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.FunctionRequestModels;
using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using FakeItEasy;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Functions
{
    [Trait("Category", "LMI import orchestration trigger function Unit Tests")]
    public class LmiImportOrchestrationTriggerTests
    {
        private readonly ILogger<LmiImportOrchestrationTrigger> fakeLogger = A.Fake<ILogger<LmiImportOrchestrationTrigger>>();
        private readonly IJobProfileService fakeJobProfileService = A.Fake<IJobProfileService>();
        private readonly IMapLmiToGraphService fakeMapLmiToGraphService = A.Fake<IMapLmiToGraphService>();
        private readonly ILmiSocImportService fakeLmiSocImportService = A.Fake<ILmiSocImportService>();
        private readonly IGraphService fakeGraphService = A.Fake<IGraphService>();
        private readonly IEventGridService fakeEventGridService = A.Fake<IEventGridService>();
        private readonly EventGridClientOptions dummyEventGridClientOptions = A.Dummy<EventGridClientOptions>();
        private readonly IDurableOrchestrationContext fakeDurableOrchestrationContext = A.Fake<IDurableOrchestrationContext>();
        private readonly LmiImportOrchestrationTrigger lmiImportOrchestrationTrigger;

        public LmiImportOrchestrationTriggerTests()
        {
            lmiImportOrchestrationTrigger = new LmiImportOrchestrationTrigger(fakeLogger, fakeJobProfileService, fakeMapLmiToGraphService, fakeLmiSocImportService, fakeGraphService, fakeEventGridService, dummyEventGridClientOptions);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGraphRefreshSocOrchestratorIsSuccessful()
        {
            // Arrange
            const bool expectedResult = true;
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { Soc = 3435 });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<bool>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await lmiImportOrchestrationTrigger.GraphRefreshSocOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.GraphPurgeSocActivity), A<int>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<bool>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostGraphEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGraphRefreshSocOrchestratorReturnsFalse()
        {
            // Arrange
            const bool expectedResult = false;
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { Soc = 3435 });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<bool>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await lmiImportOrchestrationTrigger.GraphRefreshSocOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.GraphPurgeSocActivity), A<int>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<bool>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostGraphEventActivity), A<EventGridPostRequestModel>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGraphPurgeSocOrchestratorIsSuccessful()
        {
            // Arrange
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { Soc = 3435 });

            // Act
            await lmiImportOrchestrationTrigger.GraphPurgeSocOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.GraphPurgeSocActivity), A<int>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostGraphEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGraphPurgeOrchestratorIsSuccessful()
        {
            // Arrange

            // Act
            await lmiImportOrchestrationTrigger.GraphPurgeOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.GraphPurgeActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostGraphEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGraphRefreshOrchestratorIsSuccessful()
        {
            // Arrange
            const int mappingItemsCount = 2;
            var dummyMappings = A.CollectionOfDummy<SocJobProfileMappingModel>(mappingItemsCount);
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).Returns(dummyMappings);
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<bool>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).Returns(true);

            // Act
            await lmiImportOrchestrationTrigger.GraphRefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.GraphPurgeActivity), null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<bool>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustHaveHappened(mappingItemsCount, Times.Exactly);
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostGraphEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGraphRefreshOrchestratorIsSuccessfulWhenNoMappings()
        {
            // Arrange
            const int mappingItemsCount = 0;
            var dummyMappings = A.CollectionOfDummy<SocJobProfileMappingModel>(mappingItemsCount);
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).Returns(dummyMappings);

            // Act
            await lmiImportOrchestrationTrigger.GraphRefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.GraphPurgeActivity), null)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<bool>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostGraphEventActivity), A<EventGridPostRequestModel>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGraphRefreshOrchestratorIsSuccessfulWhenNullMappings()
        {
            // Arrange
            List<SocJobProfileMappingModel>? nullMappings = null;
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).Returns(nullMappings);

            // Act
            await lmiImportOrchestrationTrigger.GraphRefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.GraphPurgeActivity), null)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<bool>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostGraphEventActivity), A<EventGridPostRequestModel>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGetJobProfileSocMappingsActivityIsSuccessful()
        {
            // Arrange
            const int mappingItemsCount = 2;
            var dummyMappings = A.CollectionOfDummy<SocJobProfileMappingModel>(mappingItemsCount);
            A.CallTo(() => fakeJobProfileService.GetMappingsAsync()).Returns(dummyMappings);

            // Act
            var results = await lmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobProfileService.GetMappingsAsync()).MustHaveHappenedOnceExactly();
            Assert.Equal(dummyMappings, results);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGraphPurgeActivityIsSuccessful()
        {
            // Arrange

            // Act
            await lmiImportOrchestrationTrigger.GraphPurgeActivity(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeGraphService.PurgeAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGraphPurgeSocActivityIsSuccessful()
        {
            // Arrange

            // Act
            await lmiImportOrchestrationTrigger.GraphPurgeSocActivity(1234).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeGraphService.PurgeSocAsync(A<int>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerImportSocItemActivityIsSuccessful()
        {
            // Arrange
            const bool expectedResult = true;
            var dummyLmiSocDatasetModel = A.Dummy<LmiSocDatasetModel>();
            var dummyGraphSocDatasetModel = A.Dummy<GraphSocDatasetModel>();
            var socJobProfileMapping = new SocJobProfileMappingModel { Soc = 1234 };

            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).Returns(dummyLmiSocDatasetModel);
            A.CallTo(() => fakeMapLmiToGraphService.Map(A<LmiSocDatasetModel>.Ignored)).Returns(dummyGraphSocDatasetModel);
            A.CallTo(() => fakeGraphService.ImportAsync(A<GraphSocDatasetModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await lmiImportOrchestrationTrigger.ImportSocItemActivity(socJobProfileMapping).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapLmiToGraphService.Map(A<LmiSocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeGraphService.ImportAsync(A<GraphSocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerImportSocItemActivityReturnsFalse()
        {
            // Arrange
            const bool expectedResult = false;
            LmiSocDatasetModel? nullLmiSocDatasetModel = null;
            var socJobProfileMapping = new SocJobProfileMappingModel { Soc = 1234 };

            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).Returns(nullLmiSocDatasetModel);

            // Act
            var result = await lmiImportOrchestrationTrigger.ImportSocItemActivity(socJobProfileMapping).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapLmiToGraphService.Map(A<LmiSocDatasetModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeGraphService.ImportAsync(A<GraphSocDatasetModel>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerPostGraphEventActivityIsSuccessful()
        {
            // Arrange
            var eventGridPostRequest = new EventGridPostRequestModel
            {
                Soc = 1234,
                DisplayText = "Display text",
                EventType = "published",
            };

            // Act
            await lmiImportOrchestrationTrigger.PostGraphEventActivity(eventGridPostRequest).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<WebhookCacheOperation>.Ignored, A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
