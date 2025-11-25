using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noodle.Api.Models;
using Noodle.Api.Repositories;

namespace Noodle.Api.Services
{
    public interface IComparisonService
    {
        Task<ComparisonResponse> CompareAsync(ComparisonRequest req);
    }
    public class ComparisonService : IComparisonService
    {
        private readonly IComparisonRepository _repository;

        public ComparisonService(IComparisonRepository repository)
        {
            _repository = repository;
        }

        public async Task<ComparisonResponse> CompareAsync(ComparisonRequest req)
        {
            if (req.AssetIds == null || req.AssetIds.Count < 2)
                throw new Exception("At least two assets must be provided.");

            if (!req.IsValidType())
                throw new Exception("Invalid asset type. Must be stablecoin, stock, or commodity.");

            List<ComparisonAsset> assets = req.AssetType.ToLower() switch
            {
                "stablecoin" => await _repository.GetStablecoinsAsync(req.AssetIds),
                "stock" => await _repository.GetStocksAsync(req.AssetIds),
                "commodity" => await _repository.GetCommoditiesAsync(req.AssetIds),
                _ => throw new Exception("Unsupported asset type.")
            };

            if (assets.Count == 0)
                throw new Exception("No matching assets found.");

            var summary = BuildSummary(assets);

            return new ComparisonResponse
            {
                ComparisonType = req.AssetType,
                Assets = assets,
                Summary = summary
            };
        }

        private ComparisonSummary BuildSummary(List<ComparisonAsset> assets)
        {
            var summary = new ComparisonSummary();

            summary.HighestMarketCap = assets
                .OrderByDescending(a => a.Metrics.MarketCap ?? 0)
                .First().Id;

            summary.LowestMarketCap = assets
                .OrderBy(a => a.Metrics.MarketCap ?? 0)
                .First().Id;

            summary.FastestGrowth = assets
                .OrderByDescending(a => a.Metrics.Growth24h ?? 0)
                .First().Id;

            // % Differences (compare asset 0 with others)
            var baseMcap = assets[0].Metrics.MarketCap ?? 0;

            summary.PercentageDifferences = assets.ToDictionary(
                x => x.Id,
                x =>
                {
                    var m = x.Metrics.MarketCap ?? 0;
                    return baseMcap == 0 ? 0 : ((m - baseMcap) / baseMcap) * 100;
                }
            );

            return summary;
        }
    }
}