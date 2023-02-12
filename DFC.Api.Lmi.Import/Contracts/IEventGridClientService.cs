using Azure.Messaging.EventGrid;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IEventGridClientService
    {
        Task SendEventAsync(List<EventGridEvent>? eventGridEvents, string? topicEndpoint, string? topicKey, string? logMessage);
    }
}
