using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Functions
{
    public class LmiWebhookHttpTrigger
    {
        private readonly ILogger<LmiWebhookHttpTrigger> logger;
        private readonly EnvironmentValues environmentValues;
        private readonly ILmiWebhookReceiverService lmiWebhookReceiverService;

        public LmiWebhookHttpTrigger(
           ILogger<LmiWebhookHttpTrigger> logger,
           EnvironmentValues environmentValues,
           ILmiWebhookReceiverService lmiWebhookReceiverService)
        {
            this.logger = logger;
            this.environmentValues = environmentValues;
            this.lmiWebhookReceiverService = lmiWebhookReceiverService;
        }

        [FunctionName("LmiWebhook")]
        [Display(Name = "LMI Webhook", Description = "Receives webhook Post requests for LMI refresh.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Processed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data or wrong environment", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "lmi/webhook")] HttpRequest? request,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            try
            {
                logger.LogInformation("Received webhook request");

                bool isDraftEnvironment = environmentValues.IsDraftEnvironment;
                using var streamReader = new StreamReader(request?.Body!);
                var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(requestBody))
                {
                    logger.LogError($"{nameof(request)} body is null");
                    return new BadRequestResult();
                }

                string? instanceId = null;
                var webhookRequestModel = lmiWebhookReceiverService.ExtractEvent(requestBody);
                switch (webhookRequestModel.WebhookCommand)
                {
                    case WebhookCommand.SubscriptionValidation:
                        return new OkObjectResult(webhookRequestModel.SubscriptionValidationResponse);
                    case WebhookCommand.PublishFromDraft:
                        if (!isDraftEnvironment)
                        {
                            return new BadRequestResult();
                        }

                        instanceId = await starter.StartNewAsync(nameof(LmiImportOrchestrationTrigger.RefreshPublishedOrchestrator)).ConfigureAwait(false);
                        break;
                    case WebhookCommand.PurgeFromPublished:
                        if (!isDraftEnvironment)
                        {
                            return new BadRequestResult();
                        }

                        instanceId = await starter.StartNewAsync(nameof(LmiImportOrchestrationTrigger.PurgePublishedOrchestrator)).ConfigureAwait(false);
                        break;
                    default:
                        return new BadRequestResult();
                }

                logger.LogInformation($"Started orchestration with ID = '{instanceId}' for {webhookRequestModel.WebhookCommand}");

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
