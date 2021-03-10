using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Functions
{
    public class LmiImportTimerTrigger
    {
        private readonly ILogger<LmiImportTimerTrigger> logger;

        public LmiImportTimerTrigger(ILogger<LmiImportTimerTrigger> logger)
        {
            this.logger = logger;
        }

        [FunctionName("LmiImportTimerTrigger")]
        public async Task Run(
            [TimerTrigger("%LmiImportTimerTriggerSchedule%")] TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            string instanceId = await starter.StartNewAsync(nameof(LmiImportOrchestrationTrigger.GraphRefreshOrchestrator), null).ConfigureAwait(false);

            logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            logger.LogTrace($"Next run of {nameof(LmiImportTimerTrigger)}is {myTimer?.ScheduleStatus?.Next}");
        }
    }
}