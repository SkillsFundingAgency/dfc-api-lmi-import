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
    [Trait("Category", "Graph refresh http trigger function Unit Tests")]
    public class GraphRefreshHttpTriggerTests
    {
        private readonly ILogger<GraphRefreshHttpTrigger> fakeLogger = A.Fake<ILogger<GraphRefreshHttpTrigger>>();
        private readonly ILmiImportService fakeLmiImportService = A.Fake<ILmiImportService>();
        private readonly GraphRefreshHttpTrigger graphRefreshHttpTrigger;

        public GraphRefreshHttpTriggerTests()
        {
            graphRefreshHttpTrigger = new GraphRefreshHttpTrigger(fakeLogger, fakeLmiImportService);
        }

        [Fact]
        public async Task LmiImportTimerTriggerRunFunctionIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            // Act
            var result = await graphRefreshHttpTrigger.Run(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiImportService.ImportAsync()).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<OkResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostReturnsUnprocessableEntityWhenUpsertRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.InternalServerError;

            A.CallTo(() => fakeLmiImportService.ImportAsync()).Throws<Exception>();

            // Act
            var result = await graphRefreshHttpTrigger.Run(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiImportService.ImportAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }
    }
}
