using DFC.Api.Lmi.Import.Functions;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Functions
{
    public class HealthPingHttpTriggerTests
    {
        private readonly ILogger logger = A.Fake<ILogger>();

        [Fact]
        public async Task HealthPingHttpTriggerTestsReturnsOk()
        {
            // Arrange

            // Act
            var result = await HealthPing.Run(new DefaultHttpRequest(new DefaultHttpContext()), logger).ConfigureAwait(false);

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}
