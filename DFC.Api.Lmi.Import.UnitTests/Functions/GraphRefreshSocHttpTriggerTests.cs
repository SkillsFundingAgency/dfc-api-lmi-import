using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Functions;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Functions
{
    [Trait("Category", "Graph refresh SOC http trigger function Unit Tests")]
    public class GraphRefreshSocHttpTriggerTests
    {
        private readonly ILogger<GraphRefreshSocHttpTrigger> fakeLogger = A.Fake<ILogger<GraphRefreshSocHttpTrigger>>();
        private readonly IGraphService fakeGraphService = A.Fake<IGraphService>();
        private readonly ILmiImportService fakeLmiImportService = A.Fake<ILmiImportService>();
        private readonly GraphRefreshSocHttpTrigger graphRefreshSocHttpTrigger;

        public GraphRefreshSocHttpTriggerTests()
        {
            graphRefreshSocHttpTrigger = new GraphRefreshSocHttpTrigger(fakeLogger, fakeGraphService, fakeLmiImportService);
        }

        [Fact]
        public async Task LmiImportTimerTriggerRunFunctionIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            // Act
            var result = await graphRefreshSocHttpTrigger.Run(null, 3231).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeGraphService.PurgeSocAsync(A<int>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiImportService.ImportItemAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<OkResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostReturnsUnprocessableEntityWhenUpsertRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.InternalServerError;

            A.CallTo(() => fakeLmiImportService.ImportItemAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).Throws<Exception>();

            // Act
            var result = await graphRefreshSocHttpTrigger.Run(null, 3231).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeGraphService.PurgeSocAsync(A<int>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeLmiImportService.ImportItemAsync(A<int>.Ignored, A<List<SocJobProfileItemModel>>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }
    }
}
