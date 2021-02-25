using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using DFC.Api.Lmi.Import.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Services
{
    [Trait("Category", "LMI SOC import service Unit Tests")]
    public class LmiSocImportServiceTests
    {
        private readonly ILogger<LmiSocImportService> fakeLogger = A.Fake<ILogger<LmiSocImportService>>();
        private readonly LmiApiClientOptions lmiApiClientOptions = new LmiApiClientOptions { BaseAddress = new Uri("https://somewhere.com", UriKind.Absolute) };
        private readonly ILmiApiConnector fakeLmiApiConnector = A.Fake<ILmiApiConnector>();
        private readonly LmiSocImportService lmiSocImportService;

        public LmiSocImportServiceTests()
        {
            lmiSocImportService = new LmiSocImportService(fakeLogger, lmiApiClientOptions, fakeLmiApiConnector);
        }

        [Fact]
        public async Task LmiSocImportServiceImportReturnsSuccess()
        {
            // arrange
            var socJobProfileMapping = new SocJobProfileMappingModel
            {
                Soc = 3231,
                JobProfiles = A.CollectionOfDummy<SocJobProfileItemModel>(2).ToList(),
            };

            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiSocDatasetModel>(A<Uri>.Ignored)).Returns(A.Dummy<LmiSocDatasetModel>());
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiPredictedModel>(A<Uri>.Ignored)).Returns(A.Dummy<LmiPredictedModel>());
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiBreakdownModel>(A<Uri>.Ignored)).Returns(A.Dummy<LmiBreakdownModel>());
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiBreakdownModel>(A<Uri>.Ignored)).Returns(A.Dummy<LmiBreakdownModel>());
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiBreakdownModel>(A<Uri>.Ignored)).Returns(A.Dummy<LmiBreakdownModel>());

            // act
            var result = await lmiSocImportService.ImportAsync(socJobProfileMapping).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiSocDatasetModel>(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiPredictedModel>(A<Uri>.Ignored)).MustHaveHappened(2, Times.Exactly);
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiBreakdownModel>(A<Uri>.Ignored)).MustHaveHappened(3, Times.Exactly);
            Assert.NotNull(result);
            Assert.NotNull(result.JobProfiles);
            Assert.NotNull(result.JobGrowth);
            Assert.NotNull(result.QualificationLevel);
            Assert.NotNull(result.EmploymentByRegion);
            Assert.NotNull(result.TopIndustriesInJobGroup);
        }

        [Fact]
        public async Task LmiSocImportServiceImportReturnsExceptionForNullJobProfileSummaries()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await lmiSocImportService.ImportAsync(null).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiSocDatasetModel>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiPredictedModel>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeLmiApiConnector.ImportAsync<LmiBreakdownModel>(A<Uri>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'socJobProfileMappingModel')", exceptionResult.Message);
        }
    }
}
