using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Connectors
{
    public class LmiApiConnector : ILmiApiConnector
    {
        private readonly ILogger<LmiApiConnector> logger;
        private readonly HttpClient httpClient;
        private readonly IApiDataConnector apiDataConnector;

        public LmiApiConnector(
            ILogger<LmiApiConnector> logger,
            HttpClient httpClient,
            IApiDataConnector apiDataConnector)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.apiDataConnector = apiDataConnector;
        }

        public async Task<TModel?> ImportAsync<TModel>(Uri uri)
            where TModel : class
        {
            logger.LogInformation($"Getting LMI data from: {uri}");

            var apiData = await apiDataConnector.GetAsync<TModel>(httpClient, uri).ConfigureAwait(false);

            if (apiData != null)
            {
                logger.LogInformation($"Sanitizing LMI data from: {uri}");

                TextSanitizerUtilities.Sanitize(apiData);

                logger.LogInformation($"Get LMI data from: {uri} was successful");

                return apiData;
            }

            return default;
        }
    }
}
