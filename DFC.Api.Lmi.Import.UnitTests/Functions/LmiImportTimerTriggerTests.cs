using DFC.Api.Lmi.Import.Common;
using DFC.Api.Lmi.Import.Functions;
using DFC.Api.Lmi.Import.Models.FunctionRequestModels;
using FakeItEasy;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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
        private readonly IDurableOrchestrationClient fakeDurableOrchestrationClient = A.Fake<IDurableOrchestrationClient>();
        private readonly TimerInfo timerInfo = new TimerInfo(new ConstantSchedule(new TimeSpan(1)), new ScheduleStatus());
        private readonly LmiImportTimerTrigger lmiImportTimerTrigger;

        public LmiImportTimerTriggerTests()
        {
            lmiImportTimerTrigger = new LmiImportTimerTrigger(fakeLogger);
            Environment.SetEnvironmentVariable(Constants.EnvironmentNameApiSuffix, "(draft)");
        }

        [Fact]
        public async Task LmiImportTimerTriggerRunFunctionIsSuccessful()
        {
            // Arrange

            // Act
            await lmiImportTimerTrigger.Run(timerInfo, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<OrchestratorRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
