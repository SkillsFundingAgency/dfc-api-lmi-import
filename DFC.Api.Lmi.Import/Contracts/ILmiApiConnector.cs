using System;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ILmiApiConnector
    {
        Task<TModel?> ImportAsync<TModel>(Uri uri)
            where TModel : class;
    }
}
