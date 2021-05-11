using DFC.Api.Lmi.Import.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IGenericGraphQueryService
    {
        Task<List<TModel>> ExecuteCypherQuery<TModel>(GraphReplicaSet graphReplicaSet, string query)
            where TModel : class, new();
    }
}
