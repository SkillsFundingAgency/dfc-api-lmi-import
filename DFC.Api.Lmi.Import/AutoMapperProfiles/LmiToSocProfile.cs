using AutoMapper;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocDataset;
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
            CreateMap<LmiSocDatasetModel, SocDatasetModel>()
                .ForMember(d => d.Id, s => s.MapFrom(m => Guid.NewGuid()));

            CreateMap<SocJobProfileItemModel, JobProfileModel>();

            CreateMap<LmiPredictedModel, PredictedModel>()
                .ForMember(d => d.Measure, s => s.MapFrom(m => "employment"));

            CreateMap<LmiReplacementDemandModel, ReplacementDemandModel>()
                .ForMember(d => d.Measure, s => s.MapFrom(m => "replacement"));

            CreateMap<LmiPredictedYearModel, PredictedYearModel>();

            CreateMap<LmiBreakdownModel, BreakdownModel>()
                .ForMember(d => d.Measure, s => s.MapFrom(m => m.Breakdown));

            CreateMap<LmiBreakdownYearModel, BreakdownYearModel>();

            CreateMap<LmiBreakdownYearValueModel, BreakdownYearValueModel>();
        }
    }
}
