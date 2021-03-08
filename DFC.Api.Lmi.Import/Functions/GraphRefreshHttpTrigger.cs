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
    public class GraphRefreshHttpTrigger
    {
        private readonly ILogger<GraphRefreshHttpTrigger> logger;
        private readonly ILmiImportService lmiImportService;

        public GraphRefreshHttpTrigger(
           ILogger<GraphRefreshHttpTrigger> logger,
           ILmiImportService lmiImportService)
        {
            this.logger = logger;
            this.lmiImportService = lmiImportService;
        }

        [FunctionName("GraphRefresh")]
        [Display(Name = "Graph refresh", Description = "Receives Post requests for graph refresh.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Refresh processed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "graph/refresh")] HttpRequest? request)
        {
            try
            {
                logger.LogInformation("Received graph refresh request");

                await lmiImportService.ImportAsync().ConfigureAwait(false);

                logger.LogInformation("Graph refresh request completed");

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
