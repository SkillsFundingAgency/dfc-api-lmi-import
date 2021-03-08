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
    [Trait("Category", "Graph purge SOC http trigger function Unit Tests")]
    public class GraphPurgeSocHttpTriggerTests
    {
        private readonly ILogger<GraphPurgeSocHttpTrigger> fakeLogger = A.Fake<ILogger<GraphPurgeSocHttpTrigger>>();
        private readonly IGraphService fakeGraphService = A.Fake<IGraphService>();
        private readonly GraphPurgeSocHttpTrigger graphPurgeSocHttpTrigger;

        public GraphPurgeSocHttpTriggerTests()
        {
            graphPurgeSocHttpTrigger = new GraphPurgeSocHttpTrigger(fakeLogger, fakeGraphService);
        }

        [Fact]
        public async Task LmiImportTimerTriggerRunFunctionIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            // Act
            var result = await graphPurgeSocHttpTrigger.Run(null, 3231).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeGraphService.PurgeSocAsync(A<int>.Ignored)).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<OkResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostReturnsUnprocessableEntityWhenUpsertRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.InternalServerError;

            A.CallTo(() => fakeGraphService.PurgeSocAsync(A<int>.Ignored)).Throws<Exception>();

            // Act
            var result = await graphPurgeSocHttpTrigger.Run(null, 3231).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeGraphService.PurgeSocAsync(A<int>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }
    }
}
