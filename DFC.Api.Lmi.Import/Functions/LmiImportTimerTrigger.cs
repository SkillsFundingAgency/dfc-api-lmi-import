using DFC.Api.Lmi.Import.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
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

        [FunctionName("GetLmiImportTimerTrigger")]
        public async Task Run([TimerTrigger("%LmiImportTimerTriggerSchedule%")] TimerInfo myTimer)
        {
            //TODO: ian: need to set Activity.CurrentActivity a bit cleaner for timer triggers - add to DFC.Compui.Telemetry ??
            using var activity = new Activity(nameof(LmiImportTimerTrigger));
            activity.SetParentId(Guid.NewGuid().ToString());
            activity.DisplayName = Environment.GetEnvironmentVariable("ApplicationName") ?? nameof(LmiImportTimerTrigger);
            activity.Start();

            await lmiImportService.ImportAsync().ConfigureAwait(false);

            logger.LogTrace($"Next run of {nameof(LmiImportTimerTrigger)}is {myTimer?.ScheduleStatus?.Next}");

            activity.Stop();
        }
    }
}