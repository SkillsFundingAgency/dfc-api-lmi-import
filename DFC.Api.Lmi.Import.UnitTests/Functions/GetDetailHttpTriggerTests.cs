using DFC.Api.Lmi.Import.Functions;
using DFC.Api.Lmi.Import.Models.SocDataset;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Functions
{
    [Trait("Category", "GetDetail - http trigger function Unit Tests")]
    public class GetDetailHttpTriggerTests
    {
        private readonly ILogger<GetDetailHttpTrigger> fakeLogger = A.Fake<ILogger<GetDetailHttpTrigger>>();
        private readonly IDocumentService<SocDatasetModel> fakeDocumentService = A.Fake<IDocumentService<SocDatasetModel>>();
        private readonly GetDetailHttpTrigger getDetailHttpTrigger;

        private Guid socId = Guid.NewGuid();

        public GetDetailHttpTriggerTests()
        {
            getDetailHttpTrigger = new GetDetailHttpTrigger(fakeLogger, fakeDocumentService);
        }

        [Fact]
        public async Task GetDetailHttpTriggerRunReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var dummyModel = A.Dummy<SocDatasetModel>();

            A.CallTo(() => fakeDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).Returns(dummyModel);

            // Act
            var result = await getDetailHttpTrigger.Run(A.Fake<HttpRequest>(), socId).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetDetailHttpTriggerRunReturnsNoContent()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            SocDatasetModel? nullModel = default;

            A.CallTo(() => fakeDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).Returns(nullModel);

            // Act
            var result = await getDetailHttpTrigger.Run(A.Fake<HttpRequest>(), socId).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }
    }
}
