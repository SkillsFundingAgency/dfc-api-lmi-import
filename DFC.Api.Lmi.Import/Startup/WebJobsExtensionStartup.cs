using AzureFunctions.Extensions.Swashbuckle;
using DFC.Api.Lmi.Import.Connectors;
using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Extensions;
using DFC.Api.Lmi.Import.HttpClientPolicies;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using DFC.Api.Lmi.Import.Services;
using DFC.Api.Lmi.Import.Startup;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.Swagger.Standard;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[assembly: WebJobsStartup(typeof(WebJobsExtensionStartup), "Web Jobs Extension Startup")]

namespace DFC.Api.Lmi.Import.Startup
{
    [ExcludeFromCodeCoverage]
    public class WebJobsExtensionStartup : IWebJobsStartup
    {
        private const string AppSettingsPolicies = "Policies";

        public void Configure(IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

        //    builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddHttpClient();
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddAutoMapper(typeof(WebJobsExtensionStartup).Assembly);
            builder.Services.AddSingleton(new EnvironmentValues());
            builder.Services.AddSingleton(new SocJobProfilesMappingsCachedModel());
            builder.Services.AddSingleton(configuration.GetSection(nameof(EventGridClientOptions)).Get<EventGridClientOptions>() ?? new EventGridClientOptions());
            builder.Services.AddSingleton(configuration.GetSection(nameof(LmiApiClientOptions)).Get<LmiApiClientOptions>() ?? new LmiApiClientOptions());
            builder.Services.AddSingleton(configuration.GetSection(nameof(JobProfileApiClientOptions)).Get<JobProfileApiClientOptions>() ?? new JobProfileApiClientOptions());
            builder.Services.AddSingleton(configuration.GetSection(nameof(GraphOptions)).Get<GraphOptions>() ?? new GraphOptions());
            builder.Services.AddGraphCluster(options => configuration.GetSection(Neo4jOptions.Neo4j).Bind(options));
            builder.Services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddTransient<IApiConnector, ApiConnector>();
            builder.Services.AddTransient<IApiDataConnector, ApiDataConnector>();
            builder.Services.AddTransient<IGraphConnector, GraphConnector>();
            builder.Services.AddTransient<ICypherQueryBuilderService, CypherQueryBuilderService>();
            builder.Services.AddTransient<ILmiSocImportService, LmiSocImportService>();
            builder.Services.AddTransient<IJobProfileService, JobProfileService>();
            builder.Services.AddTransient<IJobProfilesToSocMappingService, JobProfilesToSocMappingService>();
            builder.Services.AddTransient<IGraphService, GraphService>();
            builder.Services.AddTransient<IMapLmiToGraphService, MapLmiToGraphService>();
            builder.Services.AddTransient<IEventGridService, EventGridService>();
            builder.Services.AddTransient<IEventGridClientService, EventGridClientService>();
            builder.Services.AddTransient<IGenericGraphQueryService, GenericGraphQueryService>();

            var policyOptions = configuration.GetSection(AppSettingsPolicies).Get<PolicyOptions>() ?? new PolicyOptions();
            var policyRegistry = builder.Services.AddPolicyRegistry();

            builder.Services
                .AddPolicies(policyRegistry, nameof(LmiApiClientOptions), policyOptions)
                .AddHttpClient<ILmiApiConnector, LmiApiConnector, LmiApiClientOptions>(nameof(LmiApiClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            builder.Services
                .AddPolicies(policyRegistry, nameof(JobProfileApiClientOptions), policyOptions)
                .AddHttpClient<IJobProfileApiConnector, JobProfileApiConnector, JobProfileApiClientOptions>(nameof(JobProfileApiClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));
        }
    }
}
