﻿using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Services
{
    public class EventGridService : IEventGridService
    {
        private readonly ILogger<EventGridService> logger;
        private readonly IEventGridClientService eventGridClientService;
        private readonly EventGridClientOptions eventGridClientOptions;

        public EventGridService(ILogger<EventGridService> logger, IEventGridClientService eventGridClientService, EventGridClientOptions eventGridClientOptions)
        {
            this.logger = logger;
            this.eventGridClientService = eventGridClientService;
            this.eventGridClientOptions = eventGridClientOptions;
        }

        public async Task SendEventAsync(EventGridEventData? eventGridEventData, string? subject, string? eventType)
        {
            _ = eventGridEventData ?? throw new ArgumentNullException(nameof(eventGridEventData));

            if (!IsValidEventGridClientOptions(eventGridClientOptions))
            {
                logger.LogWarning($"Unable to send to event grid due to invalid {nameof(eventGridClientOptions)} options");
                return;
            }

            logger.LogInformation($"Sending Event Grid message for: {eventGridEventData.DisplayText}");

            var eventGridEvents = new List<EventGridEvent>
            {
                new EventGridEvent(
                   subject,
                   eventType,
                   "1.0",
                   eventGridEventData),
            };

            //eventGridEvents.ForEach(f => f.Validate());

            await eventGridClientService.SendEventAsync(eventGridEvents, eventGridClientOptions.TopicEndpoint, eventGridClientOptions.TopicKey, subject).ConfigureAwait(false);
        }

        public bool IsValidEventGridClientOptions(EventGridClientOptions? eventGridClientOptions)
        {
            _ = eventGridClientOptions ?? throw new ArgumentNullException(nameof(eventGridClientOptions));

            if (string.IsNullOrWhiteSpace(eventGridClientOptions.TopicEndpoint))
            {
                logger.LogWarning($"{nameof(eventGridClientOptions)} is missing a value for: {nameof(eventGridClientOptions.TopicEndpoint)}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(eventGridClientOptions.TopicKey))
            {
                logger.LogWarning($"{nameof(eventGridClientOptions)} is missing a value for: {nameof(eventGridClientOptions.TopicKey)}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(eventGridClientOptions.SubjectPrefix))
            {
                logger.LogWarning($"{nameof(eventGridClientOptions)} is missing a value for: {nameof(eventGridClientOptions.SubjectPrefix)}");
                return false;
            }

            if (eventGridClientOptions.ApiEndpoint == null)
            {
                logger.LogWarning($"{nameof(eventGridClientOptions)} is missing a value for: {nameof(eventGridClientOptions.ApiEndpoint)}");
                return false;
            }

            return true;
        }
    }
}
