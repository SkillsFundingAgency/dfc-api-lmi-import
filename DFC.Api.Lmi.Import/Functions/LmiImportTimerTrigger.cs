using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.FunctionRequestModels;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Functions
{
    public class LmiImportTimerTrigger
    {
        private readonly ILogger<LmiImportTimerTrigger> logger;
        private readonly EnvironmentValues environmentValues;

        public LmiImportTimerTrigger(ILogger<LmiImportTimerTrigger> logger, EnvironmentValues environmentValues)
        {
            this.logger = logger;
            this.environmentValues = environmentValues;
        }

        [FunctionName("LmiImportTimerTrigger")]
        public async Task Run(
            [TimerTrigger("%LmiImportTimerTriggerSchedule%")] TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            var orchestratorRequestModel = new OrchestratorRequestModel
            {
                IsDraftEnvironment = environmentValues.IsDraftEnvironment,
                SuccessRelayPercent = environmentValues.SuccessRelayPercent,
            };

            if (orchestratorRequestModel.IsDraftEnvironment)
            {
                string instanceId = await starter.StartNewAsync(nameof(LmiImportOrchestrationTrigger.CacheRefreshOrchestrator), orchestratorRequestModel).ConfigureAwait(false);

                logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            }

            logger.LogTrace($"Next run of {nameof(LmiImportTimerTrigger)}is {myTimer?.ScheduleStatus?.Next}");
        }
    }
}