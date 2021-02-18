using DFC.Api.Lmi.Import.Models.ClientOptions;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ILmiApiConnector
    {
        Task<TModel?> ImportAsync<TModel>(int soc, LmiApiClientOptions.LmiQuery lmiQuery)
            where TModel : class;
    }
}
