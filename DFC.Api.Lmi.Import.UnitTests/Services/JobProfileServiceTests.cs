using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.JobProfileApi;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using DFC.Api.Lmi.Import.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Services
{
    [Trait("Category", "Job profile service Unit Tests")]
    public class JobProfileServiceTests
    {
        private readonly ILogger<JobProfileService> fakeLogger = A.Fake<ILogger<JobProfileService>>();
        private readonly IJobProfileApiConnector fakeJobProfileApiConnector = A.Fake<IJobProfileApiConnector>();
        private readonly IJobProfilesToSocMappingService fakeJobProfilesToSocMappingService = A.Fake<IJobProfilesToSocMappingService>();
        private readonly JobProfileService jobProfileService;

        public JobProfileServiceTests()
        {
            jobProfileService = new JobProfileService(fakeLogger, fakeJobProfileApiConnector, fakeJobProfilesToSocMappingService);
        }

        [Fact]
        public async Task JobProfileServiceGetMappingsAsyncReturnsSuccess()
        {
            //arrange
            const int jobProfilesCount = 2;
            A.CallTo(() => fakeJobProfileApiConnector.GetSummaryAsync()).Returns(A.CollectionOfDummy<JobProfileSummaryModel>(jobProfilesCount));
            A.CallTo(() => fakeJobProfileApiConnector.GetDetailsAsync(A<IList<JobProfileSummaryModel>>.Ignored)).Returns(A.CollectionOfDummy<JobProfileDetailModel>(jobProfilesCount));
            A.CallTo(() => fakeJobProfilesToSocMappingService.Map(A<IList<JobProfileDetailModel>>.Ignored)).Returns(A.CollectionOfDummy<SocJobProfileMappingModel>(jobProfilesCount));

            //act
            var results = await jobProfileService.GetMappingsAsync().ConfigureAwait(false);

            //assert
            A.CallTo(() => fakeJobProfileApiConnector.GetSummaryAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobProfileApiConnector.GetDetailsAsync(A<IList<JobProfileSummaryModel>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobProfilesToSocMappingService.Map(A<IList<JobProfileDetailModel>>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.NotNull(results);
        }

        [Fact]
        public async Task JobProfileServiceGetMappingsAsyncReturnsNullWhenNoData()
        {
            //arrange
            IList<JobProfileSummaryModel>? nullJobProfileSumaries = null;
            A.CallTo(() => fakeJobProfileApiConnector.GetSummaryAsync()).Returns(nullJobProfileSumaries);

            //act
            var results = await jobProfileService.GetMappingsAsync().ConfigureAwait(false);

            //assert
            A.CallTo(() => fakeJobProfileApiConnector.GetSummaryAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobProfileApiConnector.GetDetailsAsync(A<IList<JobProfileSummaryModel>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobProfilesToSocMappingService.Map(A<IList<JobProfileDetailModel>>.Ignored)).MustNotHaveHappened();

            Assert.Null(results);
        }
    }
}
