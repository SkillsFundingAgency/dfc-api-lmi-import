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
    public class GraphRefreshSocHttpTrigger
    {
        private readonly ILogger<GraphRefreshSocHttpTrigger> logger;
        private readonly EnvironmentValues environmentValues;

        public GraphRefreshSocHttpTrigger(ILogger<GraphRefreshSocHttpTrigger> logger, EnvironmentValues environmentValues)
        {
            this.logger = logger;
            this.environmentValues = environmentValues;
        }

        [FunctionName("GraphRefreshSoc")]
        [Display(Name = "Graph refresh SOC", Description = "Receives Post requests for graph refresh of a SOC.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.Accepted, Description = "Refresh SOC item queued for processing", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data or wrong environment", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "graph/refresh/{soc}/{socId}")] HttpRequest? request,
            int soc,
            Guid socId,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            try
            {
                var socRequest = new SocRequestModel
                {
                    Soc = soc,
                    SocId = socId,
                    IsDraftEnvironment = environmentValues.IsDraftEnvironment,
                };

                if (!socRequest.IsDraftEnvironment)
                {
                    return new BadRequestResult();
                }

                logger.LogInformation("Received graph refresh  for SOC {soc} request");

                string instanceId = await starter.StartNewAsync(nameof(LmiImportOrchestrationTrigger.GraphRefreshSocOrchestrator), socRequest).ConfigureAwait(false);

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
