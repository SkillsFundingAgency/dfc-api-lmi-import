using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IJobProfileService
    {
        Task<IList<SocJobProfileMappingModel>?> GetMappingsAsync();
    }
}
