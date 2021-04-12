using DFC.Api.Lmi.Import.Functions;
using DFC.Api.Lmi.Import.Models;
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
    [Trait("Category", "Graph purge http trigger function Unit Tests")]
    public class GraphPurgeHttpTriggerTests
    {
        private readonly ILogger<GraphPurgeHttpTrigger> fakeLogger = A.Fake<ILogger<GraphPurgeHttpTrigger>>();
        private readonly IDurableOrchestrationClient fakeDurableOrchestrationClient = A.Fake<IDurableOrchestrationClient>();
        private readonly EnvironmentValues draftEnvironmentValues = new EnvironmentValues { EnvironmentNameApiSuffix = "(draft)" };
        private readonly EnvironmentValues publishedEnvironmentValues = new EnvironmentValues { EnvironmentNameApiSuffix = string.Empty };

        [Fact]
        public async Task GraphPurgeHttpTriggerRunFunctionIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.Accepted;
            var graphPurgeHttpTrigger = new GraphPurgeHttpTrigger(fakeLogger, draftEnvironmentValues);

            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).Returns(new AcceptedResult());

            // Act
            var result = await graphPurgeHttpTrigger.Run(null, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<OrchestratorRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<AcceptedResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task GraphPurgeHttpTriggerRunFunctionReturnsBadRequestForPublishedEnvironment()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var graphPurgeHttpTrigger = new GraphPurgeHttpTrigger(fakeLogger, publishedEnvironmentValues);

            // Act
            var result = await graphPurgeHttpTrigger.Run(null, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<OrchestratorRequestModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task GraphPurgeHttpTriggerReturnsUnprocessableEntityWhenStartNewAsyncRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.InternalServerError;
            var graphPurgeHttpTrigger = new GraphPurgeHttpTrigger(fakeLogger, draftEnvironmentValues);

            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<OrchestratorRequestModel>.Ignored)).Throws<Exception>();

            // Act
            var result = await graphPurgeHttpTrigger.Run(null, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<OrchestratorRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }
    }
}
