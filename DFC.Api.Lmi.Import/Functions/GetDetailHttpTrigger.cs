using DFC.Api.Lmi.Import.Models.SocDataset;
using DFC.Compui.Cosmos.Contracts;
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
    public class GetDetailHttpTrigger
    {
        private readonly ILogger<GetDetailHttpTrigger> logger;
        private readonly IDocumentService<SocDatasetModel> documentService;

        public GetDetailHttpTrigger(
           ILogger<GetDetailHttpTrigger> logger,
           IDocumentService<SocDatasetModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        [FunctionName("GetDetail")]
        [Display(Name = "Get detail by SOC", Description = "Retrieves a lmi-for-all-data detail.")]
        [ProducesResponseType(typeof(SocDatasetModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Detail retrieved", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/lmi-for-all-data/{socId}")] HttpRequest? request, Guid socId)
        {
            logger.LogInformation($"Getting lmi-for-all-data for {socId}");

            var socDatasetModel = await documentService.GetByIdAsync(socId).ConfigureAwait(false);

            if (socDatasetModel != null)
            {
                logger.LogInformation($"Returning {socDatasetModel.Soc} lmi-for-all-data detail");

                return new OkObjectResult(socDatasetModel);
            }

            logger.LogWarning($"Failed to get lmi-for-all-data for {socId}");

            return new NoContentResult();
        }
    }
}
