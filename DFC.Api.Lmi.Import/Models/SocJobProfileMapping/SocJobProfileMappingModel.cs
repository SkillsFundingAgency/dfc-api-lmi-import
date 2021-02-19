using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.Models.SocJobProfileMapping
{
    [ExcludeFromCodeCoverage]
    public class SocJobProfileMappingModel
    {
        public int? Soc { get; set; }

        public IList<SocJobProfileItemModel>? JobProfiles { get; set; }
    }
}
