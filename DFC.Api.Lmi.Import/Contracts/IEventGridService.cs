using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IEventGridService
    {
        Task SendEventAsync(WebhookCacheOperation webhookCacheOperation, EventGridEventData? eventGridEventData, string? subject);

        bool IsValidEventGridClientOptions(EventGridClientOptions? eventGridClientOptions);
    }
}
