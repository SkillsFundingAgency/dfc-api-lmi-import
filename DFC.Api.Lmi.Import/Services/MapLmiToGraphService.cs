using AutoMapper;
using DFC.Api.Lmi.Import.Contracts;
using DFC.Api.Lmi.Import.Models.GraphData;
using DFC.Api.Lmi.Import.Models.LmiApiData;
using System.Collections.Generic;
using System.Linq;

namespace DFC.Api.Lmi.Import.Services
{
    public class MapLmiToGraphService : IMapLmiToGraphService
    {
        private readonly IMapper mapper;

        public MapLmiToGraphService(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public GraphSocDatasetModel? Map(LmiSocDatasetModel lmiSocDataset)
        {
            var graphSocDatasetModel = mapper.Map<GraphSocDatasetModel>(lmiSocDataset);

            PropogateKeys(graphSocDatasetModel);

            return graphSocDatasetModel;
        }

        private static void PropogateKeys(GraphSocDatasetModel graphSocDataset)
        {
            PropogateKeys(graphSocDataset.Soc, graphSocDataset.JobGrowth);
            PropogateKeys(graphSocDataset.Soc, graphSocDataset.QualificationLevel);
            PropogateKeys(graphSocDataset.Soc, graphSocDataset.EmploymentByRegion);
            PropogateKeys(graphSocDataset.Soc, graphSocDataset.TopIndustriesInJobGroup);
        }

        private static void PropogateKeys(int soc, GraphPredictedModel? graphPredictedModel)
        {
            if (graphPredictedModel != null)
            {
                graphPredictedModel.Soc = soc;

                PropogateKeys(graphPredictedModel.Soc, graphPredictedModel.Measure, graphPredictedModel?.PredictedEmployment);
            }
        }

        private static void PropogateKeys(int soc, string? measure, IList<GraphPredictedYearModel>? graphPredictedYears)
        {
            if (graphPredictedYears != null && graphPredictedYears.Any())
            {
                foreach (var graphPredictedYear in graphPredictedYears)
                {
                    graphPredictedYear.Soc = soc;
                    graphPredictedYear.Measure = measure;
                }
            }
        }

        private static void PropogateKeys(int soc, GraphBreakdownModel? graphBreakdown)
        {
            if (graphBreakdown != null)
            {
                graphBreakdown.Soc = soc;

                PropogateKeys(soc, graphBreakdown.Measure, graphBreakdown?.PredictedEmployment);
            }
        }

        private static void PropogateKeys(int soc, string? measure, IList<GraphBreakdownYearModel>? graphBreakdownYears)
        {
            if (graphBreakdownYears != null && graphBreakdownYears.Any())
            {
                foreach (var graphBreakdownYear in graphBreakdownYears)
                {
                    graphBreakdownYear.Soc = soc;
                    graphBreakdownYear.Measure = measure;

                    PropogateKeys(soc, measure, graphBreakdownYear.Year, graphBreakdownYear?.Breakdown);
                }
            }
        }

        private static void PropogateKeys(int soc, string? measure, int year, IList<GraphBreakdownYearValueModel>? graphBreakdownYearValues)
        {
            if (graphBreakdownYearValues != null && graphBreakdownYearValues.Any())
            {
                foreach (var graphBreakdownYearValue in graphBreakdownYearValues)
                {
                    graphBreakdownYearValue.Soc = soc;
                    graphBreakdownYearValue.Measure = measure;
                    graphBreakdownYearValue.Year = year;
                }
            }
        }
    }
}
