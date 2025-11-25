using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Noodle.Api.Data;          // DatabaseSettings (bạn đã có trong project)
using Noodle.Api.Models;
using Noodle.Api.Helpers;

namespace Noodle.Api.Repositories
{
    public interface IComparisonRepository
    {
        Task<List<ComparisonAsset>> GetStablecoinsAsync(List<string> ids);
        Task<List<ComparisonAsset>> GetStocksAsync(List<string> ids);
        Task<List<ComparisonAsset>> GetCommoditiesAsync(List<string> ids);
    }
    public class ComparisonRepository : IComparisonRepository
    {
        private readonly IMongoDatabase _db;

        public ComparisonRepository(IMongoClient client, IOptions<DatabaseSettings> dbOptions)
        {
            _db = client.GetDatabase(dbOptions.Value.DatabaseName);
        }

        // =======================
        //      STABLECOINS
        // =======================
        public async Task<List<ComparisonAsset>> GetStablecoinsAsync(List<string> ids)
        {
            var col = _db.GetCollection<dynamic>("v2-token-overview");

            var filter = Builders<dynamic>.Filter.In("info.base_currency", ids);

            var docs = await col.Find(filter).ToListAsync();

            return docs.Select(d =>
            {
                var root = d as IDictionary<string, object> ?? new Dictionary<string, object>();

                var info = root.GetValueOrDefault("info") as IDictionary<string, object>
                        ?? new Dictionary<string, object>();

                var market = root.GetValueOrDefault("market") as IDictionary<string, object>
                            ?? new Dictionary<string, object>();

                var valuation = root.GetValueOrDefault("valuation") as IDictionary<string, object>
                                ?? new Dictionary<string, object>();

                // Helper lấy string an toàn
                string? GetStr(IDictionary<string, object> dict, string key)
                    => dict.TryGetValue(key, out var val) ? val?.ToString() : null;

                return new ComparisonAsset
                {
                    Id = GetStr(root, "symbol") ?? "--",
                    Name = GetStr(info, "base_currency_desc") ?? GetStr(info, "base_currency") ?? "--",

                    Metrics = new AssetMetrics
                    {
                        Price = ToDouble(market.GetValueOrDefault("close")),
                        Volume24h = ToDouble(market.GetValueOrDefault("24h_vol_change_cmc")),
                        MarketCap = ToDouble(valuation.GetValueOrDefault("market_cap_calc")),
                        Growth24h = ToDouble(market.GetValueOrDefault("change"))
                    }
                };
            }).ToList();
        }

        // =======================
        //        STOCKS
        // =======================
        public async Task<List<ComparisonAsset>> GetStocksAsync(List<string> ids)
        {
            var col = _db.GetCollection<dynamic>("stocks");

            var builder = Builders<dynamic>.Filter;

            // ----- Default filters -----
            var baseFilters = builder.And(
                builder.In("name", ids),
                builder.Eq("marketType", "usa"),
                builder.Eq("type", "stock")
            );

            var result = await col.Find(baseFilters).ToListAsync();

            return result.Select(d =>
            {
                var root = d as IDictionary<string, object>;

                return new ComparisonAsset
                {
                    Id = root.GetValueOrDefault("symbol")?.ToString() ?? "",
                    Name = root.GetValueOrDefault("name")?.ToString()
                        ?? root.GetValueOrDefault("symbol")?.ToString()
                        ?? "Unknown",
                    Metrics = new AssetMetrics
                    {
                        Price = ToDouble(root.GetValueOrDefault("close")),
                        Volume24h = ToDouble(root.GetValueOrDefault("volume")),
                        Growth24h = ToDouble(root.GetValueOrDefault("change")),
                        MarketCap = ToDouble(root.GetValueOrDefault("market_cap_basic"))
                    }
                };
            }).ToList();
        }

        // =======================
        //      COMMODITIES
        // =======================
        public async Task<List<ComparisonAsset>> GetCommoditiesAsync(List<string> ids)
        {
            var col = _db.GetCollection<dynamic>("commodities");

            var filter = Builders<dynamic>.Filter.In("symbol", ids);

            var result = await col.Find(filter).ToListAsync();

            return result.Select(d =>
            {
                var root = d as IDictionary<string, object>;
                return new ComparisonAsset
                {
                    Id = root.GetValueOrDefault("symbol")?.ToString() ?? "",
                    Name = root.GetValueOrDefault("name")?.ToString()
                            ?? root.GetValueOrDefault("symbol")?.ToString()
                            ?? "Unknown",
                    Metrics = new AssetMetrics
                    {
                        Price = ToDouble(root.GetValueOrDefault("price")),

                        // Commodities không có volume & marketcap
                        Volume24h = null,
                        MarketCap = null,

                        // percent = biến động trong ngày (growth 24h)
                        Growth24h = ToDouble(root.GetValueOrDefault("percent")),

                        // Extra fields cho commodity
                        Weekly = ToDouble(root.GetValueOrDefault("weekly")),
                        Monthly = ToDouble(root.GetValueOrDefault("monthly")),
                        YoY = ToDouble(root.GetValueOrDefault("yoy")),
                        YTD = ToDouble(root.GetValueOrDefault("ytd"))
                    }
                };
            }).ToList();
        }

        private double? ToDouble(object obj)
        {
            if (obj == null) return null;

            if (double.TryParse(obj.ToString(), out var value))
                return value;

            // xử lý format như "8.00%"
            var cleaned = obj.ToString().Replace("%", "");
            if (double.TryParse(cleaned, out var percentValue))
                return percentValue;

            return null;
        }
    }
}