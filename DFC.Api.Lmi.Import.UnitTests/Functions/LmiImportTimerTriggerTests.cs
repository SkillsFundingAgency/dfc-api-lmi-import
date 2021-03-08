using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Functions;
using FakeItEasy;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Functions
{
    [Trait("Category", "LMI import timer trigger function Unit Tests")]
    public class LmiImportTimerTriggerTests
    {
        private readonly ILogger<LmiImportTimerTrigger> fakeLogger = A.Fake<ILogger<LmiImportTimerTrigger>>();
        private readonly ILmiImportService fakeLmiImportService = A.Fake<ILmiImportService>();
        private readonly TimerInfo timerInfo = new TimerInfo(new ConstantSchedule(new TimeSpan(1)), new ScheduleStatus());
        private readonly LmiImportTimerTrigger lmiImportTimerTrigger;

        public LmiImportTimerTriggerTests()
        {
            lmiImportTimerTrigger = new LmiImportTimerTrigger(fakeLogger, fakeLmiImportService);
        }

        [Fact]
        public async Task LmiImportTimerTriggerRunFunctionIsSuccessful()
        {
            // Arrange

            // Act
            await lmiImportTimerTrigger.Run(timerInfo).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiImportService.ImportAsync()).MustHaveHappenedOnceExactly();
        }
    }
}
