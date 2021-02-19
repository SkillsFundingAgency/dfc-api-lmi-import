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
                .ForMember(d => d.CreatedDate, s => s.Ignore());

            CreateMap<SocJobProfileItemModel, GraphJobProfileModel>();

            CreateMap<LmiPredictedModel, GraphPredictedModel>()
                .ForMember(d => d.Measure, s => s.MapFrom(m => "employment"));

            CreateMap<LmiPredictedYearModel, GraphPredictedYearModel>()
                .ForMember(d => d.Soc, s => s.Ignore())
                .ForMember(d => d.Measure, s => s.Ignore());

            CreateMap<LmiBreakdownModel, GraphBreakdownModel>()
                .ForMember(d => d.Measure, s => s.MapFrom(m => m.Breakdown));

            CreateMap<LmiBreakdownYearModel, GraphBreakdownYearModel>()
                .ForMember(d => d.Soc, s => s.Ignore())
                .ForMember(d => d.Measure, s => s.Ignore());

            CreateMap<LmiBreakdownYearItemModel, GraphBreakdownYearItemModel>()
                .ForMember(d => d.Soc, s => s.Ignore())
                .ForMember(d => d.Measure, s => s.Ignore())
                .ForMember(d => d.Year, s => s.Ignore());
        }
    }
}
