using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IGraphConnector
    {
        IList<string> BuildImportCommanda<TModel>(TModel parent)
            where TModel : class;

        IList<string> BuildPurgeCommands();

        Task RunAsync(IList<string> commands);
    }
}
