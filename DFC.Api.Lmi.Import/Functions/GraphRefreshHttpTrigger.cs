﻿using DFC.Api.Lmi.Import.Models;
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
    public class GraphRefreshHttpTrigger
    {
        private readonly ILogger<GraphRefreshHttpTrigger> logger;
        private readonly EnvironmentValues environmentValues;

        public GraphRefreshHttpTrigger(ILogger<GraphRefreshHttpTrigger> logger, EnvironmentValues environmentValues)
        {
            this.logger = logger;
            this.environmentValues = environmentValues;
        }

        [FunctionName("GraphRefresh")]
        [Display(Name = "Graph refresh", Description = "Receives Post requests for graph refresh.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.Accepted, Description = "Refresh queued for processing", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data or wrong environment", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "graph/refresh")] HttpRequest? request,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            try
            {
                var orchestratorRequestModel = new OrchestratorRequestModel
                {
                    IsDraftEnvironment = environmentValues.IsDraftEnvironment,
                    SuccessRelayPercent = environmentValues.SuccessRelayPercent,
                };

                if (!orchestratorRequestModel.IsDraftEnvironment)
                {
                    return new BadRequestResult();
                }

                logger.LogInformation("Received graph refresh request");

                string instanceId = await starter.StartNewAsync(nameof(LmiImportOrchestrationTrigger.GraphRefreshOrchestrator), orchestratorRequestModel).ConfigureAwait(false);

                logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

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
