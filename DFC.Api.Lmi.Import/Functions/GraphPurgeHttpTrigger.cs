using DFC.Api.Lmi.Import.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Functions
{
    public class GraphPurgeHttpTrigger
    {
        private readonly ILogger<GraphPurgeHttpTrigger> logger;
        private readonly IGraphService graphService;

        public GraphPurgeHttpTrigger(
           ILogger<GraphPurgeHttpTrigger> logger,
           IGraphService graphService)
        {
            this.logger = logger;
            this.graphService = graphService;
        }

        [FunctionName("GraphPurge")]
        [Display(Name = "Graph purge", Description = "Receives Post requests for graph purge.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Purge processed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "graph/purge")] HttpRequest? request)
        {
            Activity? activity = null;

            try
            {
                logger.LogInformation("Received graph purge request");

                //TODO: ian: need to initialize the telemetry properly
                if (Activity.Current == null)
                {
                    activity = new Activity(nameof(GraphPurgeHttpTrigger)).Start();
                    activity.SetParentId(Guid.NewGuid().ToString());
                }

                await graphService.PurgeAsync().ConfigureAwait(false);

                logger.LogInformation("Graph purge request completed");

                return new OkResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                activity?.Dispose();
            }
        }
    }
}
