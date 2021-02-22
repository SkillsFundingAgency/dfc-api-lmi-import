using DFC.Api.Lmi.Import.Models.GraphData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface IGraphConnector
    {
        IList<string> BuildPurgeCommands();

        IList<string> BuildImportCommands(GraphBaseSocModel? parent);

        Task RunAsync(IList<string>? commands);
    }
}
