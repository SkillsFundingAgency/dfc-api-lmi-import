﻿using DFC.Api.Lmi.Import.Contracts;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure;

namespace DFC.Api.Lmi.Import.Services
{
    [ExcludeFromCodeCoverage]
    public class EventGridClientService : IEventGridClientService
    {
        private readonly ILogger<EventGridClientService> logger;

        public EventGridClientService(ILogger<EventGridClientService> logger)
        {
            this.logger = logger;
        }

        public async Task SendEventAsync(List<EventGridEvent>? eventGridEvents, string? topicEndpoint, string? topicKey, string? logMessage)
        {
            _ = eventGridEvents ?? throw new ArgumentNullException(nameof(eventGridEvents));
            _ = topicEndpoint ?? throw new ArgumentNullException(nameof(topicEndpoint));
            _ = topicKey ?? throw new ArgumentNullException(nameof(topicKey));
            _ = logMessage ?? throw new ArgumentNullException(nameof(logMessage));

            logger.LogInformation($"Sending Event Grid message for: {logMessage}");

            try
            {
                string topicHostname = new Uri(topicEndpoint).Host;
                var topicCredentials = new AzureKeyCredential(topicKey);
                EventGridPublisherClient client = new EventGridPublisherClient( new Uri(topicEndpoint),  topicCredentials);

                await client.SendEventsAsync(eventGridEvents).ConfigureAwait(false);

                logger.LogInformation($"Sent Event Grid message for: {logMessage}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception sending Event Grid message for: {logMessage}");
            }
        }
    }
}
