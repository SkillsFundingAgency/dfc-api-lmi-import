using DFC.Api.Lmi.Import.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
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
        private readonly IGraphService graphService;

        public GraphPurgeSocHttpTrigger(
           ILogger<GraphPurgeSocHttpTrigger> logger,
           IGraphService graphService)
        {
            this.logger = logger;
            this.graphService = graphService;
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "graph/purge/{soc}")] HttpRequest? request, int soc)
        {
            try
            {
                logger.LogInformation($"Received graph purge for SOC {soc} request");

                await graphService.PurgeSocAsync(soc).ConfigureAwait(false);

                logger.LogInformation($"Graph purge for SOC {soc} request completed");

                return new OkResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
