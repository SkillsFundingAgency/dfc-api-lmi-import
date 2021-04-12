using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.JobProfileApi;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Connectors
{
    public class JobProfileApiConnector : IJobProfileApiConnector
    {
        private readonly ILogger<JobProfileApiConnector> logger;
        private readonly HttpClient httpClient;
        private readonly JobProfileApiClientOptions jobProfileApiClientOptions;
        private readonly IApiDataConnector apiDataConnector;

        public JobProfileApiConnector(
            ILogger<JobProfileApiConnector> logger,
            HttpClient httpClient,
            JobProfileApiClientOptions jobProfileApiClientOptions,
            IApiDataConnector apiDataConnector)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.jobProfileApiClientOptions = jobProfileApiClientOptions;
            this.apiDataConnector = apiDataConnector;
        }

        public async Task<IList<JobProfileSummaryModel>?> GetSummaryAsync()
        {
            var jobProfileSummaries = await apiDataConnector.GetAsync<IList<JobProfileSummaryModel>>(httpClient, jobProfileApiClientOptions.BaseAddress!).ConfigureAwait(false);

            if (jobProfileApiClientOptions.DeveloperModeMaxJobProfiles > 0)
            {
                jobProfileSummaries = jobProfileSummaries.Take(jobProfileApiClientOptions.DeveloperModeMaxJobProfiles).ToList();
            }

            return jobProfileSummaries;
        }

        public async Task<IList<JobProfileDetailModel>> GetDetailsAsync(IList<JobProfileSummaryModel>? jobProfileSummaries)
        {
            _ = jobProfileSummaries ?? throw new ArgumentNullException(nameof(jobProfileSummaries));

            var jobProfileDetails = new List<JobProfileDetailModel>();

            foreach (var jobProfileSummary in jobProfileSummaries.Where(w => w.Url != null))
            {
                logger.LogInformation($"Retrieving details from job-profiles API: {jobProfileSummary.Url}");

                var jobProfileDetail = await apiDataConnector.GetAsync<JobProfileDetailModel>(httpClient, jobProfileSummary.Url!).ConfigureAwait(false);

                if (jobProfileDetail != null)
                {
                    jobProfileDetail.CanonicalName = jobProfileSummary.Url!.ToString().Split('/').Last();
                    jobProfileDetails.Add(jobProfileDetail);
                }
            }

            return jobProfileDetails;
        }
    }
}
