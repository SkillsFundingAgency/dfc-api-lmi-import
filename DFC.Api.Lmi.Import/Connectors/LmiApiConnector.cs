using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Connectors
{
    public class LmiApiConnector : ILmiApiConnector
    {
        private readonly ILogger<LmiApiConnector> logger;
        private readonly HttpClient httpClient;
        private readonly LmiApiClientOptions lmiApiClientOptions;
        private readonly IApiDataConnector apiDataConnector;

        public LmiApiConnector(
            ILogger<LmiApiConnector> logger,
            HttpClient httpClient,
            LmiApiClientOptions lmiApiClientOptions,
            IApiDataConnector apiDataConnector)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.lmiApiClientOptions = lmiApiClientOptions;
            this.apiDataConnector = apiDataConnector;
        }

        public async Task<TModel?> ImportAsync<TModel>(int soc, LmiApiQuery lmiApiQuery)
            where TModel : class
        {
            var uri = lmiApiClientOptions.BuildApiUri(soc, lmiApiQuery);
            logger.LogInformation($"Getting LMI data from: {uri}");

            var apiData = await apiDataConnector.GetAsync<TModel>(httpClient, uri).ConfigureAwait(false);

            if (apiData != null)
            {
                logger.LogInformation($"Get LMI data from: {uri} was successful");

                return apiData;
            }

            return default;
        }
    }
}
