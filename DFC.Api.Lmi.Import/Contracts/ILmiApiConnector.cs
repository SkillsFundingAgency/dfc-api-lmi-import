using DFC.Api.Lmi.Import.Enums;
using DFC.Api.Lmi.Import.Models.ClientOptions;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ILmiApiConnector
    {
        Task<TModel?> ImportAsync<TModel>(int soc, LmiApiQuery lmiApiQuery)
            where TModel : class;
    }
}
