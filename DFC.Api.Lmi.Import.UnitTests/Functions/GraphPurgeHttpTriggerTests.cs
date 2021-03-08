using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Functions;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IGraphService fakeGraphService = A.Fake<IGraphService>();
        private readonly GraphPurgeHttpTrigger graphPurgeHttpTrigger;

        public GraphPurgeHttpTriggerTests()
        {
            graphPurgeHttpTrigger = new GraphPurgeHttpTrigger(fakeLogger, fakeGraphService);
        }

        [Fact]
        public async Task LmiImportTimerTriggerRunFunctionIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            // Act
            var result = await graphPurgeHttpTrigger.Run(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeGraphService.PurgeAsync()).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<OkResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostReturnsUnprocessableEntityWhenUpsertRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.InternalServerError;

            A.CallTo(() => fakeGraphService.PurgeAsync()).Throws<Exception>();

            // Act
            var result = await graphPurgeHttpTrigger.Run(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeGraphService.PurgeAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }
    }
}
