using AutoMapper;
using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Import.AutoMapperProfiles
{
    [ExcludeFromCodeCoverage]
    public class LmiToSocProfile : Profile
    {
        public LmiToSocProfile()
        {
            CreateMap<LmiSocDatasetModel, GraphSocDatasetModel>()
                .ForMember(d => d.CreatedDate, s => s.MapFrom(m => DateTime.UtcNow));

            CreateMap<SocJobProfileItemModel, GraphJobProfileModel>();

            CreateMap<LmiPredictedModel, GraphPredictedModel>()
                .ForMember(d => d.PredictedType, s => s.MapFrom(m => "employment prediction"));

            CreateMap<LmiPredictedYearModel, GraphPredictedYearModel>()
                .ForMember(d => d.Soc, s => s.Ignore());

            CreateMap<LmiBreakdownModel, GraphBreakdownModel>()
                .ForMember(d => d.BreakdownType, s => s.MapFrom(m => $"{m.Breakdown} breakdown"));

            CreateMap<LmiBreakdownYearModel, GraphBreakdownYearModel>()
                .ForMember(d => d.Soc, s => s.Ignore())
                .ForMember(d => d.BreakdownType, s => s.Ignore());

            CreateMap<LmiBreakdownYearItemModel, GraphBreakdownYearItemModel>()
                .ForMember(d => d.Soc, s => s.Ignore())
                .ForMember(d => d.BreakdownType, s => s.Ignore())
                .ForMember(d => d.Year, s => s.Ignore());
        }
    }
}
