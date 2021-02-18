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

            if (graphSocDatasetModel != null)
            {
                PropogateKeys(graphSocDatasetModel);
            }

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

                PropogateKeys(graphPredictedModel.Soc, graphPredictedModel?.PredictedEmployment);
            }
        }

        private static void PropogateKeys(int soc, IList<GraphPredictedYearModel>? graphPredictedYears)
        {
            if (graphPredictedYears != null && graphPredictedYears.Any())
            {
                foreach (var graphPredictedYear in graphPredictedYears)
                {
                    graphPredictedYear.Soc = soc;
                }
            }
        }

        private static void PropogateKeys(int soc, GraphBreakdownModel? graphBreakdown)
        {
            if (graphBreakdown != null)
            {
                graphBreakdown.Soc = soc;

                PropogateKeys(soc, graphBreakdown.BreakdownType, graphBreakdown?.PredictedEmployment);
            }
        }

        private static void PropogateKeys(int soc, string? breakdownType, IList<GraphBreakdownYearModel>? graphBreakdownYears)
        {
            if (graphBreakdownYears != null && graphBreakdownYears.Any())
            {
                foreach (var graphBreakdownYear in graphBreakdownYears)
                {
                    graphBreakdownYear.Soc = soc;
                    graphBreakdownYear.BreakdownType = breakdownType;

                    PropogateKeys(soc, breakdownType, graphBreakdownYear.Year, graphBreakdownYear?.Breakdown);
                }
            }
        }

        private static void PropogateKeys(int soc, string? breakdownType, int year, IList<GraphBreakdownYearItemModel>? graphBreakdownYearItems)
        {
            if (graphBreakdownYearItems != null && graphBreakdownYearItems.Any())
            {
                foreach (var graphBreakdownYearItem in graphBreakdownYearItems)
                {
                    graphBreakdownYearItem.Soc = soc;
                    graphBreakdownYearItem.BreakdownType = breakdownType;
                    graphBreakdownYearItem.Year = year;
                }
            }
        }
    }
}
