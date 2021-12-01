using DFC.Api.Lmi.Import.Functions;
using DFC.Api.Lmi.Import.Models.FunctionRequestModels;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Functions
{
    [Trait("Category", "Cache purge SOC http trigger function Unit Tests")]
    public class CachePurgeSocHttpTriggerTests
    {
        private readonly ILogger<CachePurgeSocHttpTrigger> fakeLogger = A.Fake<ILogger<CachePurgeSocHttpTrigger>>();
        private readonly IDurableOrchestrationClient fakeDurableOrchestrationClient = A.Fake<IDurableOrchestrationClient>();

        [Fact]
        public async Task CachePurgeSocHttpTriggerRunFunctionIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.Accepted;
            var cachePurgeSocHttpTrigger = new CachePurgeSocHttpTrigger(fakeLogger);

            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).Returns(new AcceptedResult());

            // Act
            var result = await cachePurgeSocHttpTrigger.Run(null, 3231, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<SocRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<AcceptedResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task CachePurgeSocHttpTriggerReturnsUnprocessableEntityWhenStartNewAsyncRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.InternalServerError;
            var cachePurgeSocHttpTrigger = new CachePurgeSocHttpTrigger(fakeLogger);

            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<SocRequestModel>.Ignored)).Throws<Exception>();

            // Act
            var result = await cachePurgeSocHttpTrigger.Run(null, 3231, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<SocRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }
    }
}
