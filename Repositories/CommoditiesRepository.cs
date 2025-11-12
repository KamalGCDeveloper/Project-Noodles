using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Noodle.Api.Models;
using Noodle.Api.Helpers;
using MongoDB.Bson;

namespace Noodle.Api.Repositories
{
    public interface ICommoditiesRepository
    {
        Task<List<TopGrowthCommodity>> GetTopGrowthCommoditiesAsync();
        Task<List<dynamic>> GetTopCommoditiesAsync();
        Task<List<MostTalkedCommodity>> GetMostTalkedAboutCommoditiesAsync();
        Task<(int Count, int NewCount7d, double Growth7dPercent)> GetCountCommoditiesAndGrowthAsync();
        Task<ActiveUsersCommodity> GetTotalActiveUsers7dCommodityAsync();
        Task<List<CommodityGroupResult>> GetTopCommoditiesAsync(string? groupFilter = null);
    }

    public class CommoditiesRepository : ICommoditiesRepository
    {
        private readonly IMongoCollection<dynamic> _commoditiesCollection;
        private readonly IMongoCollection<dynamic> _youtubeCollection;


        public CommoditiesRepository(IMongoClient client, IConfiguration config)
        {
            // ✅ Lấy database từ appsettings.json (giống StablecoinsRepository)
            var db = client.GetDatabase(config["DatabaseSettings:DatabaseName"]);
            _commoditiesCollection = db.GetCollection<dynamic>("commodities");
            _youtubeCollection = db.GetCollection<dynamic>("v4_youtube_videos");
        }

        public async Task<List<TopGrowthCommodity>> GetTopGrowthCommoditiesAsync()
        {
            var validGroups = new[] { "metals", "energy", "agricultural" };

            var filter = Builders<dynamic>.Filter.In("group", validGroups);
            var docs = await _commoditiesCollection.Find(filter).ToListAsync();

            var cleaned = new List<TopGrowthCommodity>();

            foreach (var item in docs)
            {
                try
                {
                    var dict = item as IDictionary<string, object> ?? new Dictionary<string, object>();
                    var weeklyStr = dict.GetValueOrDefault("weekly")?.ToString();
                    if (string.IsNullOrWhiteSpace(weeklyStr)) continue;

                    var growthRate = double.TryParse(
                        weeklyStr.Replace("%", "").Trim(),
                        out var parsed
                    ) ? parsed : 0;

                    cleaned.Add(new TopGrowthCommodity
                    {
                        Name = dict.GetValueOrDefault("name")?.ToString(),
                        Group = dict.GetValueOrDefault("group")?.ToString(),
                        Weekly = weeklyStr,
                        GrowthRate = growthRate,
                        Trend = dict.GetValueOrDefault("trend")?.ToString(),
                        Symbol = dict.GetValueOrDefault("symbol")?.ToString(),
                        Exchange = dict.GetValueOrDefault("exchange")?.ToString(),
                        MediumLogoUrl = dict.GetValueOrDefault("medium_logo_url")?.ToString()
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CommoditiesRepository] Skip invalid doc: {ex.Message}");
                }
            }

            return cleaned
                .OrderByDescending(x => x.GrowthRate)
                .Take(5)
                .ToList();
        }

        public async Task<List<dynamic>> GetTopCommoditiesAsync()
        {
            var validGroups = new[] { "metals", "energy", "agricultural" };

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("group", new BsonDocument("$in", new BsonArray(validGroups)))),
                new BsonDocument("$sort", new BsonDocument("price", -1)),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$group" },
                    { "topItems", new BsonDocument("$push", "$$ROOT") }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "group", "$_id" },
                    { "topItems", new BsonDocument("$slice", new BsonArray { "$topItems", 10 }) }
                })
            };

            var cursor = await _commoditiesCollection.AggregateAsync<dynamic>(pipeline);
            return await cursor.ToListAsync();
        }
        public async Task<List<MostTalkedCommodity>> GetMostTalkedAboutCommoditiesAsync()
        {
            var groups = await GetTopCommoditiesAsync();
            var allCommodities = groups
                .SelectMany(g =>
                {
                    var groupName = g.group.ToString();
                    var topItems = ((IEnumerable<dynamic>)g.topItems).Select(i => new
                    {
                        Group = groupName,
                        Name = i.name?.ToString(),
                        NameSlug = i.name_slug?.ToString(),
                        Symbol = i.symbol?.ToString(),
                        MediumLogoUrl = i.medium_logo_url?.ToString(),
                        Exchange = i.exchange?.ToString()
                    });
                    return topItems;
                })
                .ToList();

            var nameSlugs = allCommodities
                .Where(c => !string.IsNullOrEmpty(c.NameSlug))
                .Select(c => c.NameSlug)
                .ToList();

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("hashtag", new BsonDocument("$in", new BsonArray(nameSlugs)))),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$hashtag" },
                    { "mentions", new BsonDocument("$sum", 1) }
                })
            };

            var cursor = await _youtubeCollection.AggregateAsync<dynamic>(pipeline);
            var mentionCounts = await cursor.ToListAsync();

            var mentionsMap = mentionCounts.ToDictionary(
                x => x._id.ToString(),
                x => (int)x.mentions
            );

            var top = allCommodities
                .Select(c => new MostTalkedCommodity
                {
                    Name = c.Name,
                    NameSlug = c.NameSlug,
                    Group = c.Group,
                    Symbol = c.Symbol,
                    MediumLogoUrl = c.MediumLogoUrl,
                    Exchange = c.Exchange,
                    Mentions = mentionsMap.ContainsKey(c.NameSlug ?? "") ? mentionsMap[c.NameSlug] : 0
                })
                .OrderByDescending(c => c.Mentions)
                .Take(5)
                .Select((c, i) => { c.Rank = i + 1; return c; })
                .ToList();

            return top;
        }

        public async Task<(int Count, int NewCount7d, double Growth7dPercent)> GetCountCommoditiesAndGrowthAsync()
        {
            var collection = _commoditiesCollection;
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).Date;

            int count = 0;
            int newCount7d = 0;

            using var cursor = await collection.FindAsync(
                FilterDefinition<dynamic>.Empty,
                new FindOptions<dynamic, dynamic>
                {
                    Projection = Builders<dynamic>.Projection.Include("date"),
                    BatchSize = 1000
                }
            );

            while (await cursor.MoveNextAsync())
            {
                foreach (var doc in cursor.Current)
                {
                    count++;
                    if (doc is IDictionary<string, object> dict && dict.TryGetValue("date", out var dateObj))
                    {
                        if (DateTime.TryParse(dateObj?.ToString(), out var dateValue) && dateValue >= sevenDaysAgo)
                            newCount7d++;
                    }
                }
            }

            var count7dAgo = count - newCount7d;
            double growth7dPercent = count7dAgo > 0
                ? Math.Round((double)newCount7d / count7dAgo * 100, 2)
                : 100;

            return (count, newCount7d, growth7dPercent);
        }
        public async Task<ActiveUsersCommodity> GetTotalActiveUsers7dCommodityAsync()
        {
            var random = new Random();
            int total = random.Next(1_000_000, 5_000_000); // 1M - 5M
            int last7days = random.Next(50_000, 300_000);  // 50k - 300k

            int previousTotal = total - last7days;
            double percent = previousTotal > 0
                ? Math.Round((double)last7days / previousTotal * 100, 2)
                : 0;

            var direction = percent == 0 ? "no-change" : percent > 0 ? "up" : "down";

            var result = new ActiveUsersCommodity
            {
                Value = total,
                Change = new ActiveUsersChangeData
                {
                    Absolute = last7days,
                    Percentage = percent,
                    Direction = direction
                }
            };

            return await Task.FromResult(result);
        }
        public async Task<List<CommodityGroupResult>> GetTopCommoditiesAsync(string? groupFilter = null)
        {
            var validGroups = new[] { "metals", "energy", "agricultural" };
            var result = new List<CommodityGroupResult>();

            // Nếu có filter thì không dùng group pipeline
            if (!string.IsNullOrEmpty(groupFilter))
            {
                var baseQuery = new List<BsonDocument>
        {
            new("$match", new BsonDocument("group", groupFilter)),
            new("$sort", new BsonDocument("price", -1)),
            new("$limit", 10)
        };

                // Lấy trực tiếp danh sách commodity items
                var items = await _commoditiesCollection.Aggregate<CommodityItem>(baseQuery).ToListAsync();

                // Enrich từng item
                var enrichedItems = items.Select(item =>
                {
                    double ParsePercent(string? val)
                    {
                        if (string.IsNullOrEmpty(val)) return 0;
                        if (double.TryParse(val.Replace("%", "").Trim(), out var num))
                            return num;
                        return 0;
                    }

                    var weekly = ParsePercent(item.Weekly);
                    var monthly = ParsePercent(item.Monthly);
                    var ytd = ParsePercent(item.Ytd);
                    var yoy = ParsePercent(item.Yoy);

                    var healthScore = Math.Round(
                        weekly * 0.2 + monthly * 0.3 + ytd * 0.3 + yoy * 0.2, 2
                    );

                    item.HealthScore = healthScore;
                    item.EnergyType = item.Group == "energy" ? "" : "";

                    return item;
                }).ToList();

                // Gói lại thành 1 nhóm để giữ format nhất quán
                result.Add(new CommodityGroupResult
                {
                    Group = groupFilter,
                    TopItems = enrichedItems
                });
            }
            else
            {
                // Không có filter → group theo group field
                var baseQuery = new List<BsonDocument>
        {
            new("$match", new BsonDocument("group", new BsonDocument("$in", new BsonArray(validGroups)))),
            new("$sort", new BsonDocument { { "group", 1 }, { "price", -1 } }),
            new("$group", new BsonDocument
            {
                { "_id", "$group" },
                { "topItems", new BsonDocument("$push", "$$ROOT") }
            }),
            new("$project", new BsonDocument
            {
                { "_id", 0 },
                { "group", "$_id" },
                { "topItems", new BsonDocument("$slice", new BsonArray { "$topItems", 10 }) }
            })
        };

                var raw = await _commoditiesCollection.Aggregate<CommodityGroupResult>(baseQuery).ToListAsync();

                foreach (var group in raw)
                {
                    var enrichedItems = group.TopItems.Select(item =>
                    {
                        double ParsePercent(string? val)
                        {
                            if (string.IsNullOrEmpty(val)) return 0;
                            if (double.TryParse(val.Replace("%", "").Trim(), out var num))
                                return num;
                            return 0;
                        }

                        var weekly = ParsePercent(item.Weekly);
                        var monthly = ParsePercent(item.Monthly);
                        var ytd = ParsePercent(item.Ytd);
                        var yoy = ParsePercent(item.Yoy);

                        var healthScore = Math.Round(
                            weekly * 0.2 + monthly * 0.3 + ytd * 0.3 + yoy * 0.2, 2
                        );

                        item.HealthScore = healthScore;
                        item.EnergyType = item.Group == "energy" ? "" : "";

                        return item;
                    }).ToList();

                    result.Add(new CommodityGroupResult
                    {
                        Group = group.Group,
                        TopItems = enrichedItems
                    });
                }
            }

            return result;
        }
    }
}