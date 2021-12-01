using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.FunctionRequestModels;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
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
            _ = starter ?? throw new ArgumentNullException(nameof(starter));

            var orchestratorRequestModel = new OrchestratorRequestModel
            {
                SuccessRelayPercent = environmentValues.SuccessRelayPercent,
            };

            string instanceId = await starter.StartNewAsync(nameof(LmiImportOrchestrationTrigger.CacheRefreshOrchestrator), orchestratorRequestModel).ConfigureAwait(false);

            logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            logger.LogTrace($"Next run of {nameof(LmiImportTimerTrigger)}is {myTimer?.ScheduleStatus?.Next}");
        }
    }
}