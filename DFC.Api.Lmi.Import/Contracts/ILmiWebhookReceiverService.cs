using DFC.Api.Lmi.Import.Models.FunctionRequestModels;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ILmiWebhookReceiverService
    {
        WebhookRequestModel ExtractEvent(string requestBody);
    }
}
