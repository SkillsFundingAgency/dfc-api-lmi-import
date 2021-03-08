using DFC.Api.Lmi.Import.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Functions
{
    public class LmiImportTimerTrigger
    {
        private readonly ILogger<LmiImportTimerTrigger> logger;
        private readonly ILmiImportService lmiImportService;

        public LmiImportTimerTrigger(
            ILogger<LmiImportTimerTrigger> logger,
            ILmiImportService lmiImportService)
        {
            this.logger = logger;
            this.lmiImportService = lmiImportService;
        }

        [FunctionName("LmiImportTimerTrigger")]
        public async Task Run([TimerTrigger("%LmiImportTimerTriggerSchedule%")] TimerInfo myTimer)
        {
            await lmiImportService.ImportAsync().ConfigureAwait(false);

            logger.LogTrace($"Next run of {nameof(LmiImportTimerTrigger)}is {myTimer?.ScheduleStatus?.Next}");
        }
    }
}