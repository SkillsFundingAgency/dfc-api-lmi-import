using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.FunctionRequestModels;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Functions
{
    public class CachePurgeSocHttpTrigger
    {
        private readonly ILogger<CachePurgeSocHttpTrigger> logger;
        private readonly EnvironmentValues environmentValues;

        public CachePurgeSocHttpTrigger(ILogger<CachePurgeSocHttpTrigger> logger, EnvironmentValues environmentValues)
        {
            this.logger = logger;
            this.environmentValues = environmentValues;
        }

        [FunctionName("CachePurgeSoc")]
        [Display(Name = "Cache purge SOC", Description = "Receives Post requests for cache purge of a SOC.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.Accepted, Description = "Purge SOC item queued for processing", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data or wrong environment", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cache/purge/{soc}")] HttpRequest? request,
            int soc,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            try
            {
                var socRequest = new SocRequestModel
                {
                    Soc = soc,
                };

                logger.LogInformation($"Received cache purge for SOC {soc} request");

                string instanceId = await starter.StartNewAsync(nameof(LmiImportOrchestrationTrigger.CachePurgeSocOrchestrator), socRequest).ConfigureAwait(false);

                logger.LogInformation($"Started orchestration with ID = '{instanceId}' for SOC {soc}.");

                return starter.CreateCheckStatusResponse(request, instanceId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
