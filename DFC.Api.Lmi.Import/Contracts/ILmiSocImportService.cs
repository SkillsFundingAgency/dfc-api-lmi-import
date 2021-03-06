﻿using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Import.Contracts
{
    public interface ILmiSocImportService
    {
        Task<LmiSocDatasetModel?> ImportAsync(int soc, List<SocJobProfileItemModel>? jobProfiles);
    }
}
