using DFC.Api.Lmi.Import.Connectors;
using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.JobProfileApi;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Connectors
{
    [Trait("Category", "Job Profile API connector Unit Tests")]
    public class JobProfileApiConnectorTests
    {
        private readonly ILogger<JobProfileApiConnector> fakeLogger = A.Fake<ILogger<JobProfileApiConnector>>();
        private readonly HttpClient httpClient = new HttpClient();
        private readonly JobProfileApiClientOptions jobProfileApiClientOptions = new JobProfileApiClientOptions { BaseAddress = new Uri("https://somewhere.com/", UriKind.Absolute) };
        private readonly IApiDataConnector fakeApiDataConnector = A.Fake<IApiDataConnector>();
        private readonly IJobProfileApiConnector jobProfileApiConnector;

        public JobProfileApiConnectorTests()
        {
            jobProfileApiConnector = new JobProfileApiConnector(fakeLogger, httpClient, jobProfileApiClientOptions, fakeApiDataConnector);
        }

        [Fact]
        public async Task JobProfileApiConnectorGetSummaryReturnsSuccess()
        {
            // arrange
            var expectedResults = new List<JobProfileSummaryModel>
            {
                new JobProfileSummaryModel
                {
                    Title = "A title",
                    Url = new Uri("https://somewhere.com/", UriKind.Absolute),
                },
            };

            A.CallTo(() => fakeApiDataConnector.GetAsync<IList<JobProfileSummaryModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(expectedResults);

            // act
            var results = await jobProfileApiConnector.GetSummaryAsync().ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiDataConnector.GetAsync<IList<JobProfileSummaryModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.NotNull(results);
            Assert.Equal(expectedResults.Count, results!.Count);
            Assert.Equal(expectedResults.First().Title, results.First().Title);
            Assert.Equal(expectedResults.First().Url, results.First().Url);
        }

        [Fact]
        public async Task JobProfileApiConnectorGetDetailsReturnsSuccess()
        {
            // arrange
            var expectedResult = new JobProfileDetailModel
            {
                Title = "A title",
                Url = new Uri("https://somewhere.com/", UriKind.Absolute),
            };
            var jobProfileSummaries = new List<JobProfileSummaryModel>
            {
                new JobProfileSummaryModel
                {
                    Title = expectedResult.Title,
                    Url = expectedResult.Url,
                },
            };

            A.CallTo(() => fakeApiDataConnector.GetAsync<JobProfileDetailModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(expectedResult);

            // act
            var results = await jobProfileApiConnector.GetDetailsAsync(jobProfileSummaries).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiDataConnector.GetAsync<JobProfileDetailModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappened(jobProfileSummaries.Count, Times.Exactly);
            Assert.NotNull(results);
            Assert.Equal(jobProfileSummaries.Count, results!.Count);
            Assert.Equal(expectedResult.Title, results.First().Title);
            Assert.Equal(expectedResult.Url, results.First().Url);
        }

        [Fact]
        public async Task JobProfileApiConnectorGetDetailsReturnsExceptionForNullJobProfileSummaries()
        {
            // arrange

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await jobProfileApiConnector.GetDetailsAsync(null).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiDataConnector.GetAsync<JobProfileDetailModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'jobProfileSummaries')", exceptionResult.Message);
        }
    }
}
