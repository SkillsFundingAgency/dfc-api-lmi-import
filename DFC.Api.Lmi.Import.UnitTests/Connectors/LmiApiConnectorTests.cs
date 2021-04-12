using DFC.Api.Lmi.Import.Connectors;
using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.UnitTests.TestModels;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Connectors
{
    [Trait("Category", "LMI API connector Unit Tests")]
    public class LmiApiConnectorTests
    {
        private readonly ILogger<LmiApiConnector> fakeLogger = A.Fake<ILogger<LmiApiConnector>>();
        private readonly HttpClient httpClient = new HttpClient();
        private readonly IApiDataConnector fakeApiDataConnector = A.Fake<IApiDataConnector>();
        private readonly ILmiApiConnector lmiApiConnector;

        public LmiApiConnectorTests()
        {
            lmiApiConnector = new LmiApiConnector(fakeLogger, httpClient, fakeApiDataConnector);
        }

        [Fact]
        public async Task LmiApiConnectorImportReturnsSuccess()
        {
            // arrange
            var expectedResult = new ApiTestModel
            {
                Id = Guid.NewGuid(),
                Name = "a name",
            };

            A.CallTo(() => fakeApiDataConnector.GetAsync<ApiTestModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(expectedResult);

            // act
            var result = await lmiApiConnector.ImportAsync<ApiTestModel>(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiDataConnector.GetAsync<ApiTestModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Id, result!.Id);
            Assert.Equal(expectedResult.Name, result.Name);
        }

        [Fact]
        public async Task LmiApiConnectorImportReturnsNullForNoData()
        {
            // arrange
            A.CallTo(() => fakeApiDataConnector.GetAsync<ApiTestModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(default(ApiTestModel));

            // act
            var result = await lmiApiConnector.ImportAsync<ApiTestModel>(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiDataConnector.GetAsync<ApiTestModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Null(result);
        }
    }
}
