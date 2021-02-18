using AutoMapper;
using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using DFC.Api.Lmi.Import.Services;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Services
{
    [Trait("Category", "Map LMI to Graph service Unit Tests")]
    public class MapLmiToGraphServiceTests
    {
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly MapLmiToGraphService mapLmiToGraphService;

        public MapLmiToGraphServiceTests()
        {
            mapLmiToGraphService = new MapLmiToGraphService(fakeMapper);
        }

        [Fact]
        public void MapLmiToGraphServiceMapReturnsSuccess()
        {
            // arrange
            var lmiSocDataset = BuildValidLmiSocDatasetModel();
            var expectedResult = BuildValidGraphSocDatasetModel();

            A.CallTo(() => fakeMapper.Map<GraphSocDatasetModel>(A<LmiSocDatasetModel>.Ignored)).Returns(expectedResult);

            // act
            var result = mapLmiToGraphService.Map(lmiSocDataset);

            // assert
            Assert.NotNull(result);
            Assert.NotNull(result!.JobProfiles);
            Assert.NotNull(result.JobGrowth!.PredictedEmployment);
            Assert.NotNull(result.QualificationLevel!.PredictedEmployment.First().Breakdown);
            Assert.NotNull(result.EmploymentByRegion!.PredictedEmployment.First().Breakdown);
            Assert.NotNull(result.TopIndustriesInJobGroup!.PredictedEmployment.First().Breakdown);
            Assert.Equal(lmiSocDataset.Soc, result.Soc);
            Assert.Equal(lmiSocDataset.JobProfiles.First().CanonicalName, result.JobProfiles.First().CanonicalName);
            Assert.Equal(lmiSocDataset.JobProfiles.First().Title, result.JobProfiles.First().Title);
            Assert.Equal(lmiSocDataset.Soc, result.JobGrowth.Soc);
            Assert.Equal(lmiSocDataset.Soc, result.JobGrowth.PredictedEmployment.First().Soc);
            Assert.Equal(lmiSocDataset.JobGrowth!.PredictedEmployment.First().Year, result.JobGrowth.PredictedEmployment.First().Year);
            Assert.Equal(lmiSocDataset.JobGrowth.PredictedEmployment.First().Employment, result.JobGrowth.PredictedEmployment.First().Employment);
            Assert.Equal(lmiSocDataset.Soc, result.QualificationLevel.Soc);
            Assert.Equal(lmiSocDataset.QualificationLevel!.Breakdown, result.QualificationLevel.BreakdownType);
            Assert.Equal(lmiSocDataset.QualificationLevel.PredictedEmployment.First().Year, result.QualificationLevel.PredictedEmployment.First().Year);
            Assert.Equal(lmiSocDataset.Soc, result.QualificationLevel.PredictedEmployment.First().Breakdown.First().Soc);
            Assert.Equal(lmiSocDataset.QualificationLevel.PredictedEmployment.First().Year, result.QualificationLevel.PredictedEmployment.First().Breakdown.First().Year);
            Assert.Equal(lmiSocDataset.QualificationLevel.Breakdown, result.QualificationLevel.PredictedEmployment.First().Breakdown.First().BreakdownType);
            Assert.Equal(lmiSocDataset.Soc, result.EmploymentByRegion.Soc);
            Assert.Equal(lmiSocDataset.EmploymentByRegion!.Breakdown, result.EmploymentByRegion.BreakdownType);
            Assert.Equal(lmiSocDataset.EmploymentByRegion.PredictedEmployment.First().Year, result.EmploymentByRegion.PredictedEmployment.First().Year);
            Assert.Equal(lmiSocDataset.Soc, result.EmploymentByRegion.PredictedEmployment.First().Breakdown.First().Soc);
            Assert.Equal(lmiSocDataset.EmploymentByRegion.PredictedEmployment.First().Year, result.EmploymentByRegion.PredictedEmployment.First().Breakdown.First().Year);
            Assert.Equal(lmiSocDataset.EmploymentByRegion.Breakdown, result.EmploymentByRegion.PredictedEmployment.First().Breakdown.First().BreakdownType);
            Assert.Equal(lmiSocDataset.Soc, result.TopIndustriesInJobGroup.Soc);
            Assert.Equal(lmiSocDataset.TopIndustriesInJobGroup!.Breakdown, result.TopIndustriesInJobGroup.BreakdownType);
            Assert.Equal(lmiSocDataset.TopIndustriesInJobGroup.PredictedEmployment.First().Year, result.TopIndustriesInJobGroup.PredictedEmployment.First().Year);
            Assert.Equal(lmiSocDataset.Soc, result.TopIndustriesInJobGroup.PredictedEmployment.First().Breakdown.First().Soc);
            Assert.Equal(lmiSocDataset.TopIndustriesInJobGroup.PredictedEmployment.First().Year, result.TopIndustriesInJobGroup.PredictedEmployment.First().Breakdown.First().Year);
            Assert.Equal(lmiSocDataset.TopIndustriesInJobGroup.Breakdown, result.QualificationLevel.PredictedEmployment.First().Breakdown.First().BreakdownType);
        }

        private LmiSocDatasetModel BuildValidLmiSocDatasetModel()
        {
            const int soc = 3231;

            return new LmiSocDatasetModel
            {
                Soc = soc,
                Title = "A title",
                AdditionalTitles = new List<string> { "Title two", "Title three" },
                JobProfiles = new List<SocJobProfileItemModel>
                {
                    new SocJobProfileItemModel
                    {
                        CanonicalName = "a-canonical-name",
                        Title = "A title",
                    },
                },
                JobGrowth = new LmiPredictedModel
                {
                    PredictedEmployment = new List<LmiPredictedYearModel>
                    {
                        new LmiPredictedYearModel
                        {
                            Year = DateTime.UtcNow.Year,
                            Employment = new decimal(123.45),
                        },
                    },
                },
                QualificationLevel = new LmiBreakdownModel
                {
                    Note = "a note",
                    Breakdown = "breakdown",
                    PredictedEmployment = new List<LmiBreakdownYearModel>
                    {
                        new LmiBreakdownYearModel
                        {
                            Year = DateTime.UtcNow.Year,
                            Breakdown = new List<LmiBreakdownYearItemModel>
                            {
                                new LmiBreakdownYearItemModel
                                {
                                    Code = 1,
                                    Note = "a note",
                                    Name = "A name",
                                    Employment = new decimal(123.45),
                                },
                            },
                        },
                    },
                },
                EmploymentByRegion = new LmiBreakdownModel
                {
                    Note = "b note",
                    Breakdown = "breakdown",
                    PredictedEmployment = new List<LmiBreakdownYearModel>
                    {
                        new LmiBreakdownYearModel
                        {
                            Year = DateTime.UtcNow.Year,
                            Breakdown = new List<LmiBreakdownYearItemModel>
                            {
                                new LmiBreakdownYearItemModel
                                {
                                    Code = 2,
                                    Note = "b note",
                                    Name = "B name",
                                    Employment = new decimal(123.45),
                                },
                            },
                        },
                    },
                },
                TopIndustriesInJobGroup = new LmiBreakdownModel
                {
                    Soc = soc,
                    Note = "c note",
                    Breakdown = "breakdown",
                    PredictedEmployment = new List<LmiBreakdownYearModel>
                    {
                        new LmiBreakdownYearModel
                        {
                            Year = DateTime.UtcNow.Year,
                            Breakdown = new List<LmiBreakdownYearItemModel>
                            {
                                new LmiBreakdownYearItemModel
                                {
                                    Code = 3,
                                    Note = "c note",
                                    Name = "C name",
                                    Employment = new decimal(123.45),
                                },
                            },
                        },
                    },
                },
            };
        }

        private GraphSocDatasetModel BuildValidGraphSocDatasetModel()
        {
            const int soc = 3231;

            return new GraphSocDatasetModel
            {
                Soc = soc,
                Title = "A title",
                AdditionalTitles = new List<string> { "Title two", "Title three" },
                JobProfiles = new List<GraphJobProfileModel>
                {
                    new GraphJobProfileModel
                    {
                        CanonicalName = "a-canonical-name",
                        Title = "A title",
                    },
                },
                JobGrowth = new GraphPredictedModel
                {
                    PredictedEmployment = new List<GraphPredictedYearModel>
                    {
                        new GraphPredictedYearModel
                        {
                            Year = DateTime.UtcNow.Year,
                            Employment = new decimal(123.45),
                        },
                    },
                },
                QualificationLevel = new GraphBreakdownModel
                {
                    Note = "a note",
                    BreakdownType = "breakdown",
                    PredictedEmployment = new List<GraphBreakdownYearModel>
                    {
                        new GraphBreakdownYearModel
                        {
                            Year = DateTime.UtcNow.Year,
                            Breakdown = new List<GraphBreakdownYearItemModel>
                            {
                                new GraphBreakdownYearItemModel
                                {
                                    Code = 1,
                                    Note = "a note",
                                    Name = "A name",
                                    Employment = new decimal(123.45),
                                },
                            },
                        },
                    },
                },
                EmploymentByRegion = new GraphBreakdownModel
                {
                    Note = "b note",
                    BreakdownType = "breakdown",
                    PredictedEmployment = new List<GraphBreakdownYearModel>
                    {
                        new GraphBreakdownYearModel
                        {
                            Year = DateTime.UtcNow.Year,
                            Breakdown = new List<GraphBreakdownYearItemModel>
                            {
                                new GraphBreakdownYearItemModel
                                {
                                    Code = 2,
                                    Note = "b note",
                                    Name = "B name",
                                    Employment = new decimal(123.45),
                                },
                            },
                        },
                    },
                },
                TopIndustriesInJobGroup = new GraphBreakdownModel
                {
                    Note = "c note",
                    BreakdownType = "breakdown",
                    PredictedEmployment = new List<GraphBreakdownYearModel>
                    {
                        new GraphBreakdownYearModel
                        {
                            Year = DateTime.UtcNow.Year,
                            Breakdown = new List<GraphBreakdownYearItemModel>
                            {
                                new GraphBreakdownYearItemModel
                                {
                                    Code = 3,
                                    Note = "c note",
                                    Name = "C name",
                                    Employment = new decimal(123.45),
                                },
                            },
                        },
                    },
                },
            };
        }
    }
}
