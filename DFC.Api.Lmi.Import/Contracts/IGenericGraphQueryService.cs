using Neo4j.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IGenericGraphQueryService
    {
        Task<IEnumerable<IRecord>> ExecuteCypherQuery(string query);
    }
}
