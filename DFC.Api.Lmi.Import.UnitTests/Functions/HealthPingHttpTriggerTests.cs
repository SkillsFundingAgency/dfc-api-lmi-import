using DFC.Api.Lmi.Import.Functions;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Functions
{
    public class HealthPingHttpTriggerTests
    {
        private readonly ILogger logger = A.Fake<ILogger>();

        [Fact]
        public void HealthPingHttpTriggerTestsReturnsOk()
        {
            // Arrange
            var context = new DefaultHttpContext();
            // Act
            var result = HealthPingHttpTrigger.Run(context.Request);
            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}
