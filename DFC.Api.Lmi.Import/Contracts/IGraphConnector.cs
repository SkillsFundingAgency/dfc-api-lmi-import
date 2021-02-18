using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IGraphConnector
    {
        IList<string> BuildPurgeCommands();

        IList<string> BuildImportCommands<TModel>(TModel? parent)
            where TModel : class;

        Task RunAsync(IList<string> commands);
    }
}
