using DFC.Api.Lmi.Import.Contracts;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Connectors
{
    public class ApiDataConnector : IApiDataConnector
    {
        private readonly IApiConnector apiService;

        public ApiDataConnector(IApiConnector apiService)
        {
            this.apiService = apiService;
        }

        public async Task<TApiModel?> GetAsync<TApiModel>(HttpClient? httpClient, Uri url)
            where TApiModel : class
        {
            _ = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            var response = await apiService.GetAsync(httpClient, url, MediaTypeNames.Application.Json).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(response))
            {
                return JsonConvert.DeserializeObject<TApiModel>(response);
            }

            return default;
        }
    }
}
