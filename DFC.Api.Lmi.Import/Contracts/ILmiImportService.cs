using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ILmiImportService
    {
        Task ImportAsync();

        Task<bool> ImportItemAsync(int soc, List<SocJobProfileItemModel>? jobProfiles);
    }
}
