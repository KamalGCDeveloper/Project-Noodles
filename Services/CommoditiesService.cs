using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noodle.Api.Models;
using Noodle.Api.Repositories;

namespace Noodle.Api.Services
{
    public interface ICommoditiesService
    {
        Task<List<TopGrowthCommodity>> GetTopGrowthCommoditiesAsync();
        Task<List<MostTalkedCommodity>> GetMostTalkedAboutCommoditiesAsync();
        Task<TrackedCommodities> GetNumberTrackedCommoditiesAsync();
        Task<ActiveUsersCommodity> GetTotalActiveUsers7dCommodityAsync();
        Task<CommoditiesListResponse> GetCommoditiesHealthRanksAsync(int limit, int page, string? groupFilter);
    }

    public class CommoditiesService : ICommoditiesService
    {
        private readonly ICommoditiesRepository _commoditiesRepository;

        public CommoditiesService(ICommoditiesRepository commoditiesRepository)
        {
            _commoditiesRepository = commoditiesRepository;
        }

        public async Task<List<TopGrowthCommodity>> GetTopGrowthCommoditiesAsync()
        {
            var commodities = await _commoditiesRepository.GetTopGrowthCommoditiesAsync();

            // Thêm rank
            var ranked = commodities.Select((p, idx) =>
            {
                p.Rank = idx + 1;
                return p;
            }).ToList();

            return ranked;
        }
        public async Task<List<MostTalkedCommodity>> GetMostTalkedAboutCommoditiesAsync()
        {
            return await _commoditiesRepository.GetMostTalkedAboutCommoditiesAsync();
        }
        public async Task<TrackedCommodities> GetNumberTrackedCommoditiesAsync()
        {
            var (count, newCount7d, growth7dPercent) = await _commoditiesRepository.GetCountCommoditiesAndGrowthAsync();

            var direction = growth7dPercent > 0 ? "up" :
                            growth7dPercent < 0 ? "down" : "no-change";

            return new TrackedCommodities
            {
                Value = count,
                Change = new ChangeData
                {
                    Absolute = newCount7d,
                    Percentage = growth7dPercent,
                    Direction = direction
                }
            };
        }
        public async Task<ActiveUsersCommodity> GetTotalActiveUsers7dCommodityAsync()
        {
            return await _commoditiesRepository.GetTotalActiveUsers7dCommodityAsync();
        }
        public async Task<CommoditiesListResponse> GetCommoditiesHealthRanksAsync(int limit, int page, string? groupFilter)
        {
            var commoditiesList = await _commoditiesRepository.GetTopCommoditiesAsync(groupFilter);

            return new CommoditiesListResponse
            {
                Data = commoditiesList,
                Metadata = new CommoditiesMetadata
                {
                    Filters = new { },
                    Pagination = new CommoditiesPagination
                    {
                        CurrentPage = page,
                        TotalPages = 1,
                        ItemsPerPage = limit,
                        TotalItems = commoditiesList.Count
                    },
                    Timestamp = DateTime.UtcNow.ToString("o"),
                    Source = "dashboard-analytics",
                    TrendChartTimespan = "7d",
                    TrendChartUnit = "day"
                }
            };
        }
    }
}