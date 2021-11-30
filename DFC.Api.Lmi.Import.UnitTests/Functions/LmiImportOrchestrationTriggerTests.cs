using AutoMapper;
using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Functions;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.FunctionRequestModels;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocDataset;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using DFC.Api.Lmi.Import.UnitTests.TestModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Functions
{
    [Trait("Category", "LMI import orchestration trigger function Unit Tests")]
    public class LmiImportOrchestrationTriggerTests
    {
        private readonly ILogger<LmiImportOrchestrationTrigger> fakeLogger = A.Fake<ILogger<LmiImportOrchestrationTrigger>>();
        private readonly IJobProfileService fakeJobProfileService = A.Fake<IJobProfileService>();
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly ILmiSocImportService fakeLmiSocImportService = A.Fake<ILmiSocImportService>();
        private readonly IDocumentService<SocDatasetModel> fakeDocumentService = A.Fake<IDocumentService<SocDatasetModel>>();
        private readonly IEventGridService fakeEventGridService = A.Fake<IEventGridService>();
        private readonly EventGridClientOptions dummyEventGridClientOptions = A.Dummy<EventGridClientOptions>();
        private readonly IDurableOrchestrationContext fakeDurableOrchestrationContext = A.Fake<IDurableOrchestrationContext>();
        private readonly SocJobProfilesMappingsCachedModel socJobProfilesMappingsCachedModel = new SocJobProfilesMappingsCachedModel();
        private readonly LmiImportOrchestrationTrigger lmiImportOrchestrationTrigger;

        public LmiImportOrchestrationTriggerTests()
        {
            lmiImportOrchestrationTrigger = new LmiImportOrchestrationTrigger(
                fakeLogger,
                fakeMapper,
                fakeJobProfileService,
                fakeLmiSocImportService,
                fakeDocumentService,
                fakeEventGridService,
                dummyEventGridClientOptions,
                socJobProfilesMappingsCachedModel);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCacheRefreshSocOrchestratorIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { Soc = 3435 });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).Returns(Guid.NewGuid());

            // Act
            var result = await lmiImportOrchestrationTrigger.CacheRefreshSocOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCacheRefreshSocOrchestratorReturnsNullItemId()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            Guid? nullGuid = null;
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { Soc = 3435 });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).Returns(nullGuid);

            // Act
            var result = await lmiImportOrchestrationTrigger.CacheRefreshSocOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCacheRefreshSocOrchestratorReturnsExceptionForNullHttpClient()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await lmiImportOrchestrationTrigger.CacheRefreshSocOrchestrator(null!).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'context')", exceptionResult.Message);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCachePurgeSocOrchestratorIsSuccessful()
        {
            // Arrange
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { Soc = 3435 });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<SocDatasetModel>(nameof(LmiImportOrchestrationTrigger.GetCachedSocDocumentActivity), A<int>.Ignored)).Returns(new SocDatasetModel { Id = Guid.NewGuid(), Soc = 3435 });

            // Act
            await lmiImportOrchestrationTrigger.CachePurgeSocOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<SocDatasetModel>(nameof(LmiImportOrchestrationTrigger.GetCachedSocDocumentActivity), A<int>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.CachePurgeSocActivity), A<Guid?>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostCacheEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCachePurgeSocOrchestratorReturnsExceptionForNullHttpClient()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await lmiImportOrchestrationTrigger.CachePurgeSocOrchestrator(null!).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'context')", exceptionResult.Message);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCachePurgeOrchestratorIsSuccessful()
        {
            // Arrange
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).Returns(new OrchestratorRequestModel { IsDraftEnvironment = true });

            // Act
            await lmiImportOrchestrationTrigger.CachePurgeOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.CachePurgeActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostCacheEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCachePurgeOrchestratorReturnsExceptionForNullHttpClient()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await lmiImportOrchestrationTrigger.CachePurgeOrchestrator(null!).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'context')", exceptionResult.Message);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCacheRefreshOrchestratorIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            const int mappingItemsCount = 2;
            var dummyMappings = A.CollectionOfDummy<SocJobProfileMappingModel>(mappingItemsCount);
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).Returns(new OrchestratorRequestModel { IsDraftEnvironment = true });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).Returns(dummyMappings);
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).Returns(Guid.NewGuid());

            // Act
            var result = await lmiImportOrchestrationTrigger.CacheRefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.CachePurgeActivity), null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustHaveHappened(mappingItemsCount, Times.Exactly);
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostCacheEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCacheRefreshOrchestratorReturnsExceptionForNullHttpClient()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await lmiImportOrchestrationTrigger.CacheRefreshOrchestrator(null!).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'context')", exceptionResult.Message);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCacheRefreshOrchestratorBadRequestWhenSuccessThresholdNotMet()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            const int mappingItemsCount = 2;
            var dummyMappings = A.CollectionOfDummy<SocJobProfileMappingModel>(mappingItemsCount);
            Guid? nullGuid = null;
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).Returns(new OrchestratorRequestModel { IsDraftEnvironment = true, SuccessRelayPercent = 99 });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).Returns(dummyMappings);
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).Returns(nullGuid);

            // Act
            var result = await lmiImportOrchestrationTrigger.CacheRefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.CachePurgeActivity), null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustHaveHappened(mappingItemsCount, Times.Exactly);
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostCacheEventActivity), A<EventGridPostRequestModel>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCacheRefreshOrchestratorIsSuccessfulWhenNoMappings()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            const int mappingItemsCount = 0;
            var dummyMappings = A.CollectionOfDummy<SocJobProfileMappingModel>(mappingItemsCount);
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).Returns(new OrchestratorRequestModel { IsDraftEnvironment = true });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).Returns(dummyMappings);

            // Act
            var result = await lmiImportOrchestrationTrigger.CacheRefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.CachePurgeActivity), null)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostCacheEventActivity), A<EventGridPostRequestModel>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCacheRefreshOrchestratorIsSuccessfulWhenNullMappings()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            List<SocJobProfileMappingModel>? nullMappings = null;
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).Returns(new OrchestratorRequestModel { IsDraftEnvironment = true });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).Returns(nullMappings);

            // Act
            var result = await lmiImportOrchestrationTrigger.CacheRefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<OrchestratorRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SocJobProfileMappingModel>?>(nameof(LmiImportOrchestrationTrigger.GetJobProfileSocMappingsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.CachePurgeActivity), null)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<Guid?>(nameof(LmiImportOrchestrationTrigger.ImportSocItemActivity), A<SocJobProfileMappingModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiImportOrchestrationTrigger.PostCacheEventActivity), A<EventGridPostRequestModel>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
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
        public async Task LmiImportOrchestrationTriggerCachePurgeActivityIsSuccessful()
        {
            // Arrange

            // Act
            await lmiImportOrchestrationTrigger.CachePurgeActivity(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.PurgeAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerCachePurgeSocActivityIsSuccessful()
        {
            // Arrange

            // Act
            await lmiImportOrchestrationTrigger.CachePurgeSocActivity(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.OK)]
        public async Task LmiImportOrchestrationTriggerImportSocItemActivityIsSuccessful(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var dummyLmiSocDatasetModel = A.Dummy<LmiSocDatasetModel>();
            var dummyCacheSocDatasetModel = A.Dummy<SocDatasetModel>();
            var socJobProfileMapping = new SocJobProfileMappingModel { Soc = 1234 };

            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).Returns(dummyLmiSocDatasetModel);
            A.CallTo(() => fakeMapper.Map<SocDatasetModel>(A<LmiSocDatasetModel>.Ignored)).Returns(dummyCacheSocDatasetModel);
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<SocDatasetModel, bool>>>.Ignored, A<string>.Ignored)).Returns(new SocDatasetModel());
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<SocDatasetModel>.Ignored)).Returns(httpStatusCode);

            // Act
            var result = await lmiImportOrchestrationTrigger.ImportSocItemActivity(socJobProfileMapping).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<SocDatasetModel>(A<LmiSocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<SocDatasetModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<SocDatasetModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerImportSocItemActivityReturnsExceptionForNullHttpClient()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await lmiImportOrchestrationTrigger.ImportSocItemActivity(null!).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'socJobProfileMapping')", exceptionResult.Message);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerImportSocItemActivityReturnsNullItemId()
        {
            // Arrange
            LmiSocDatasetModel? nullLmiSocDatasetModel = null;
            var socJobProfileMapping = new SocJobProfileMappingModel { Soc = 1234 };

            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).Returns(nullLmiSocDatasetModel);

            // Act
            var result = await lmiImportOrchestrationTrigger.ImportSocItemActivity(socJobProfileMapping).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiSocImportService.ImportAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<SocDatasetModel>(A<LmiSocDatasetModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<SocDatasetModel, bool>>>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<SocDatasetModel>.Ignored)).MustNotHaveHappened();
            Assert.Null(result);
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerGetCachedSocDocumentActivityIsSuccessful()
        {
            // Arrange
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<SocDatasetModel, bool>>>.Ignored, A<string>.Ignored)).Returns(new SocDatasetModel());

            // Act
            await lmiImportOrchestrationTrigger.GetCachedSocDocumentActivity(1234).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<SocDatasetModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerPostCacheEventActivityIsSuccessful()
        {
            // Arrange
            var eventGridPostRequest = new EventGridPostRequestModel
            {
                ItemId = Guid.NewGuid(),
                DisplayText = "Display text",
                EventType = "published",
            };

            // Act
            await lmiImportOrchestrationTrigger.PostCacheEventActivity(eventGridPostRequest).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiImportOrchestrationTriggerPostCacheEventActivityExceptionForNullHttpClient()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await lmiImportOrchestrationTrigger.PostCacheEventActivity(null).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'eventGridPostRequest')", exceptionResult.Message);
        }
    }
}
