using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ILmiImportService
    {
        Task ImportAsync();
    }
}
