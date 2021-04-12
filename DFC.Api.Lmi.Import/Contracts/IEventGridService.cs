using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IEventGridService
    {
        Task SendEventAsync(EventGridEventData? eventGridEventData, string? subject, string? eventType);

        bool IsValidEventGridClientOptions(EventGridClientOptions? eventGridClientOptions);
    }
}
