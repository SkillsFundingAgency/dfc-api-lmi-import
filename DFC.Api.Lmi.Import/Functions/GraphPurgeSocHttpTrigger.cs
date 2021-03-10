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
    public class GraphPurgeSocHttpTrigger
    {
        private readonly ILogger<GraphPurgeSocHttpTrigger> logger;

        public GraphPurgeSocHttpTrigger(ILogger<GraphPurgeSocHttpTrigger> logger)
        {
            this.logger = logger;
        }

        [FunctionName("GraphPurgeSoc")]
        [Display(Name = "Graph purge SOC", Description = "Receives Post requests for graph purge of a SOC.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Purge processed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "graph/purge/{soc}")] HttpRequest? request,
            int soc,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            try
            {
                logger.LogInformation($"Received graph purge for SOC {soc} request");

                var socRequest = new SocRequestModel { Soc = soc };
                string instanceId = await starter.StartNewAsync(nameof(LmiImportOrchestrationTrigger.GraphPurgeSocOrchestrator), socRequest).ConfigureAwait(false);

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
