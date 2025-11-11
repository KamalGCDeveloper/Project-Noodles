using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver;
using Noodle.Api.Models;
using Noodle.Api.Data;
using Noodle.Api.Constants;
using Noodle.Api.Helpers;
using MongoDB.Bson;

namespace Noodle.Api.Repositories
{
	public interface IStablecoinsRepository
	{
		Task<List<TopGrowthStablecoin>> FindTopStablecoinsByMentionGrowthAsync();
		Task<List<MostTalkedStablecoin>> FindTopStablecoinsByMentionsAsync();
		Task<(int total, int newCount7d, double growth7dPercent)> GetStablecoinCountAndGrowthAsync();
		Task<(double total, double last7days, double percent)> CountAndRecentUsersStablecoinsAsync();
		Task<(List<StablecoinItem> Rows, int Total)> FindStablecoinsAsync(string? q, int skip, int limit);
	}

	public class StablecoinsRepository : IStablecoinsRepository
	{
		private readonly IMongoCollection<dynamic> _collection;

		public StablecoinsRepository(IMongoClient client, IConfiguration config)
		{
			var db = client.GetDatabase(config["DatabaseSettings:DatabaseName"]);
			_collection = db.GetCollection<dynamic>("v2-token-overview");
		}

		public async Task<List<TopGrowthStablecoin>> FindTopStablecoinsByMentionGrowthAsync()
		{
			// ========== excludedSymbols ==========
			var excludedSymbols = StablecoinConstants.ExcludedSymbols;
			var forceInclude = StablecoinConstants.ForceInclude;

			// ========== Build orConds ==========
			var orConds = new List<FilterDefinition<dynamic>>
						{
								Builders<dynamic>.Filter.In("info.crypto_common_categories_tr", new[] { "Stablecoins" })
						};

			foreach (var f in forceInclude)
			{
				var andCond = Builders<dynamic>.Filter.And(
						Builders<dynamic>.Filter.Eq("info.base_currency", f.base_currency),
						Builders<dynamic>.Filter.Eq("info.base_currency_desc", f.base_currency_desc)
				);
				orConds.Add(andCond);
			}

			// ========== Build baseFilter ==========
			var baseFilter = Builders<dynamic>.Filter.And(
					Builders<dynamic>.Filter.Or(orConds),
					Builders<dynamic>.Filter.Nin("symbol", excludedSymbols),
					Builders<dynamic>.Filter.Gt("market.open", 0),
					Builders<dynamic>.Filter.Gt("market.close", 0)
			);

			// ========== Projection ==========
			var projection = Builders<dynamic>.Projection
					.Include("symbol")
					.Include("info.base_currency")
					.Include("info.base_currency_desc")
					.Include("info.base_currency_logoid")
					.Include("market.open")
					.Include("market.close")
					.Include("market.change")
					.Include("market.perf_w")
					.Include("market.perf_1m")
					.Include("market.24h_vol_cmc")
					.Include("mentioned_x_tweets")
					.Include("mentioned_x_tweets_7d");

			var docs = await _collection
					.Find(baseFilter)
					.Project<dynamic>(projection)
					.ToListAsync();

			// ========== Parse result safely ==========
			var result = new List<TopGrowthStablecoin>();

			foreach (var d in docs)
			{
				try
				{
					var root = d as IDictionary<string, object> ?? new Dictionary<string, object>();

					// ----- symbol -----
					var symbol = root.TryGetValue("symbol", out var symbolObj)
							? symbolObj?.ToString() ?? string.Empty
							: string.Empty;

					// ----- info -----
					string baseCurrency = string.Empty;
					string baseCurrencyDesc = string.Empty;
					string baseCurrencyLogoId = string.Empty;

					if (root.TryGetValue("info", out var infoObj) && infoObj is IDictionary<string, object> info)
					{
						if (info.TryGetValue("base_currency", out var bc))
							baseCurrency = bc?.ToString() ?? string.Empty;

						if (info.TryGetValue("base_currency_desc", out var bcDesc))
							baseCurrencyDesc = bcDesc?.ToString() ?? string.Empty;

						if (info.TryGetValue("base_currency_logoid", out var logoId))
							baseCurrencyLogoId = logoId?.ToString() ?? string.Empty;
					}

					// ----- market -----
					double open = 0, close = 0, change = 0;

					if (root.TryGetValue("market", out var marketObj) && marketObj is IDictionary<string, object> market)
					{
						if (market.TryGetValue("open", out var openObj))
							open = ToDouble(openObj);

						if (market.TryGetValue("close", out var closeObj))
							close = ToDouble(closeObj);

						if (market.TryGetValue("change", out var changeObj))
							change = ToDouble(changeObj);
						else if (open > 0)
							change = ((close - open) / open) * 100;
					}

					double growth = Math.Round(change, 2);

					// ----- mentions -----
					double mentions = 0, mentions7d = 0;

					if (root.TryGetValue("mentioned_x_tweets", out var mObj))
						mentions = ToDouble(mObj);

					if (root.TryGetValue("mentioned_x_tweets_7d", out var m7Obj))
						mentions7d = ToDouble(m7Obj);

					// ----- Build model -----
					result.Add(new TopGrowthStablecoin
					{
						Name = baseCurrency,
						Description = baseCurrencyDesc,
						Symbol = symbol,
						Logo = string.IsNullOrWhiteSpace(baseCurrencyLogoId)
									? string.Empty
									: $"https://s3-symbol-logo.tradingview.com/{baseCurrencyLogoId}.svg",
						GrowthRate7d = growth,
						Mentions = mentions,
						Mentions7d = mentions7d
					});
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[StablecoinsRepository] Skip invalid document: {ex.Message}");
				}
			}

			// ========== Sort & Limit ==========
			return result
					.OrderByDescending(x => x.GrowthRate7d)
					.Take(5)
					.ToList();
		}

		public async Task<List<MostTalkedStablecoin>> FindTopStablecoinsByMentionsAsync()
		{
			var excludedSymbols = StablecoinConstants.ExcludedSymbols;
			var forceInclude = StablecoinConstants.ForceInclude;

			var orConds = new List<FilterDefinition<dynamic>>
		{
				Builders<dynamic>.Filter.In("info.crypto_common_categories_tr", new[] { "Stablecoins" })
		};

			foreach (var f in forceInclude)
			{
				var andCond = Builders<dynamic>.Filter.And(
						Builders<dynamic>.Filter.Eq("info.base_currency", f.base_currency),
						Builders<dynamic>.Filter.Eq("info.base_currency_desc", f.base_currency_desc)
				);
				orConds.Add(andCond);
			}

			var baseFilter = Builders<dynamic>.Filter.And(
					Builders<dynamic>.Filter.Or(orConds),
					Builders<dynamic>.Filter.Nin("symbol", excludedSymbols)
			);

			var projection = Builders<dynamic>.Projection
					.Include("symbol")
					.Include("crypto_total_rank")
					.Include("info.base_currency")
					.Include("info.base_currency_desc")
					.Include("info.base_currency_logoid")
					.Include("mentioned_x_tweets");

			var docs = await _collection
					.Find(baseFilter)
					.Project<dynamic>(projection)
					.Sort(Builders<dynamic>.Sort.Descending("mentioned_x_tweets"))
					.Limit(5)
					.ToListAsync();

			var result = new List<MostTalkedStablecoin>();

			foreach (var d in docs)
			{
				try
				{
					var root = d as IDictionary<string, object> ?? new Dictionary<string, object>();

					int rank = 0;
					if (root.TryGetValue("crypto_total_rank", out var rankObj))
						rank = Convert.ToInt32(rankObj);

					string symbol = root.TryGetValue("symbol", out var sym) ? sym?.ToString() ?? "" : "";
					string baseCurrency = "", baseCurrencyDesc = "", logoId = "";

					if (root.TryGetValue("info", out var infoObj) && infoObj is IDictionary<string, object> info)
					{
						baseCurrency = info.TryGetValue("base_currency", out var bc) ? bc?.ToString() ?? "" : "";
						baseCurrencyDesc = info.TryGetValue("base_currency_desc", out var bd) ? bd?.ToString() ?? "" : "";
						logoId = info.TryGetValue("base_currency_logoid", out var lid) ? lid?.ToString() ?? "" : "";
					}

					double mentions = 0;
					if (root.TryGetValue("mentioned_x_tweets", out var m))
						mentions = ToDouble(m);

					result.Add(new MostTalkedStablecoin
					{
						Rank = rank,
						Name = baseCurrency,
						Description = baseCurrencyDesc,
						Symbol = symbol,
						Mentions = mentions,
						Logo = string.IsNullOrWhiteSpace(logoId) ? "" : $"https://s3-symbol-logo.tradingview.com/{logoId}.svg"
					});
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[StablecoinsRepository] Skip invalid doc (mentions): {ex.Message}");
				}
			}

			return result;
		}

		public async Task<(int total, int newCount7d, double growth7dPercent)> GetStablecoinCountAndGrowthAsync()
		{
			var excludedSymbols = StablecoinConstants.ExcludedSymbols;
			var forceInclude = StablecoinConstants.ForceInclude;

			var tokenCol = _collection.Database.GetCollection<dynamic>("v2-token");
			var overviewCol = _collection.Database.GetCollection<dynamic>("v2-token-overview");

			var now = DateTime.UtcNow;
			var sevenDaysAgo = now.AddDays(-7); // ✅ đúng 7 ngày (Elysia dùng nhầm 30 ngày)

			// ---- Filter ----
			var orConds = new List<FilterDefinition<dynamic>>
		{
				Builders<dynamic>.Filter.In("info.crypto_common_categories_tr", new[] { "Stablecoins" })
		};

			foreach (var f in forceInclude)
			{
				var andCond = Builders<dynamic>.Filter.And(
						Builders<dynamic>.Filter.Eq("info.base_currency", f.base_currency),
						Builders<dynamic>.Filter.Eq("info.base_currency_desc", f.base_currency_desc)
				);
				orConds.Add(andCond);
			}

			var baseFilter = Builders<dynamic>.Filter.And(
					Builders<dynamic>.Filter.Or(orConds),
					Builders<dynamic>.Filter.Nin("symbol", excludedSymbols)
			);

			// ---- 1. Tổng số stablecoin ----
			var total = (int)await overviewCol.CountDocumentsAsync(baseFilter);

			// ---- 2. Các symbol cập nhật trong 7 ngày ----
			var filterRecent = Builders<dynamic>.Filter.Gte("updatedAt", sevenDaysAgo);
			var recentUpdatedSymbols = await tokenCol.DistinctAsync<string>("symbol", filterRecent);
			var recentList = recentUpdatedSymbols.ToList();

			// ---- 3. Đếm stablecoin mới ----
			var inRecent = Builders<dynamic>.Filter.In("symbol", recentList);
			var newCount7d = (int)await overviewCol.CountDocumentsAsync(
					Builders<dynamic>.Filter.And(baseFilter, inRecent)
			);

			var count7dAgo = total - newCount7d;
			double growth7dPercent = count7dAgo > 0
					? Math.Round((double)newCount7d / count7dAgo * 100, 2)
					: 100;

			return (total, newCount7d, growth7dPercent);
		}

		public async Task<(double total, double last7days, double percent)> CountAndRecentUsersStablecoinsAsync()
		{
			var excludedSymbols = StablecoinConstants.ExcludedSymbols;
			var forceInclude = StablecoinConstants.ForceInclude;

			var overviewCol = _collection.Database.GetCollection<dynamic>("v2-token-overview");

			// ----- 1. Lọc các Stablecoins -----
			var orConds = new List<FilterDefinition<dynamic>>
		{
				Builders<dynamic>.Filter.In("info.crypto_common_categories_tr", new[] { "Stablecoins" })
		};

			foreach (var f in forceInclude)
			{
				var andCond = Builders<dynamic>.Filter.And(
						Builders<dynamic>.Filter.Eq("info.base_currency", f.base_currency),
						Builders<dynamic>.Filter.Eq("info.base_currency_desc", f.base_currency_desc)
				);
				orConds.Add(andCond);
			}

			var baseFilter = Builders<dynamic>.Filter.And(
					Builders<dynamic>.Filter.Or(orConds),
					Builders<dynamic>.Filter.Nin("symbol", excludedSymbols),
					Builders<dynamic>.Filter.Gt("holders.addresses_active", 0)
			);

			var projection = Builders<dynamic>.Projection
					.Include("symbol")
					.Include("holders.addresses_active")
					.Include("holders.addresses_new")
					.Include("holders.total_addresses_with_balance");

			// ----- 2. Lấy danh sách stablecoins -----
			var docs = await overviewCol
					.Find(baseFilter)
					.Project<dynamic>(projection)
					.ToListAsync();

			double totalActiveUsers7d = 0;
			double increasedUsers7d = 0;

			foreach (var d in docs)
			{
				var root = d as IDictionary<string, object> ?? new Dictionary<string, object>();

				if (root.TryGetValue("holders", out var holdersObj) && holdersObj is IDictionary<string, object> holders)
				{
					double active = 0;
					double newlyActive = 0;

					if (holders.TryGetValue("addresses_active", out var a))
						active = ToDouble(a);

					if (holders.TryGetValue("addresses_new", out var n))
						newlyActive = ToDouble(n);

					totalActiveUsers7d += active;
					increasedUsers7d += newlyActive;
				}
			}

			double pastUsers = totalActiveUsers7d - increasedUsers7d;
			double growth7dPercent = pastUsers > 0
					? Math.Round((increasedUsers7d / pastUsers) * 100, 2)
					: 0;

			return (totalActiveUsers7d, increasedUsers7d, growth7dPercent);
		}

		public async Task<(List<StablecoinItem> Rows, int Total)> FindStablecoinsAsync(string? q, int skip, int limit)
		{
			var excludedSymbols = StablecoinConstants.ExcludedSymbols;
			var forceInclude = StablecoinConstants.ForceInclude;

			// ---- 2 collections ----
			var db = _collection.Database;
			var overviewCol = db.GetCollection<dynamic>("v2-token-overview");
			var tokenCol = db.GetCollection<dynamic>("v2-token");

			// ---- Filter cơ bản ----
			var orConds = new List<FilterDefinition<dynamic>>
		{
				Builders<dynamic>.Filter.In("info.crypto_common_categories_tr", new[] { "Stablecoins" })
		};

			foreach (var f in forceInclude)
			{
				var andCond = Builders<dynamic>.Filter.And(
						Builders<dynamic>.Filter.Eq("info.base_currency", f.base_currency),
						Builders<dynamic>.Filter.Eq("info.base_currency_desc", f.base_currency_desc)
				);
				orConds.Add(andCond);
			}

			var baseFilter = Builders<dynamic>.Filter.And(
					Builders<dynamic>.Filter.Or(orConds),
					Builders<dynamic>.Filter.Nin("symbol", excludedSymbols)
			);

			// ---- Search (nếu có q) ----
			if (!string.IsNullOrWhiteSpace(q))
			{
				var regex = new BsonRegularExpression(q.Trim(), "i");
				var searchFilter = Builders<dynamic>.Filter.Or(
						Builders<dynamic>.Filter.Regex("symbol", regex),
						Builders<dynamic>.Filter.Regex("info.base_currency", regex),
						Builders<dynamic>.Filter.Regex("info.base_currency_desc", regex)
				);
				baseFilter = Builders<dynamic>.Filter.And(baseFilter, searchFilter);
			}

			// ---- Projection ----
			var projectionOverview = Builders<dynamic>.Projection
					.Include("symbol")
					.Include("info.base_currency")
					.Include("info.base_currency_desc")
					.Include("info.base_currency_logoid")
					.Include("info.crypto_common_categories_tr")
					.Include("info.circulating_supply")
					.Include("info.max_supply")
					.Include("info.total_supply")
					.Include("market.close")
					.Include("market.24h_vol_cmc")
					.Include("valuation.market_cap_calc")
					.Include("valuation.crypto_total_rank")
					.Include("crypto_total_rank");

			// ---- Query song song ----
			var totalTask = overviewCol.CountDocumentsAsync(baseFilter);
			var overviewTask = overviewCol
					.Find(baseFilter)
					.Project<dynamic>(projectionOverview)
					.Sort(Builders<dynamic>.Sort.Ascending("valuation.crypto_total_rank").Ascending("symbol"))
					.Skip(skip)
					.Limit(limit)
					.ToListAsync();

			await Task.WhenAll(totalTask, overviewTask);

			var total = (int)totalTask.Result;
			var baseRows = overviewTask.Result;

			// ---- Lấy allowedBases từ overview ----
			var allowedBases = new HashSet<string>();
			foreach (var row in baseRows)
			{
				if (row is IDictionary<string, object> root &&
						root.TryGetValue("info", out var infoObj) &&
						infoObj is IDictionary<string, object> info)
				{
					var baseCur = info.GetValueOrDefault("base_currency")?.ToString()?.ToUpper();
					if (!string.IsNullOrEmpty(baseCur))
						allowedBases.Add(baseCur);
				}
			}

			// ---- Lấy token docs từ v2-token ----
			var tokenFilter = Builders<dynamic>.Filter.In("base_currency", allowedBases);
			var projectionToken = Builders<dynamic>.Projection
					.Include("base_currency")
					.Include("base_currency_desc")
					.Include("mechanism")
					.Include("depegging_history")
					.Include("brief_info")
					.Include("market_cap_calc")
					.Include("24h_vol_cmc");

			var tokenDocs = await tokenCol.Find(tokenFilter).Project<dynamic>(projectionToken).ToListAsync();

			// ---- Index token docs ----
			var byPair = new Dictionary<string, IDictionary<string, object>>();
			var byBase = new Dictionary<string, IDictionary<string, object>>();

			foreach (var t in tokenDocs)
			{
				if (t is IDictionary<string, object> td)
				{
					var baseCur = td.GetValueOrDefault("base_currency")?.ToString()?.ToUpper() ?? "";
					var desc = td.GetValueOrDefault("base_currency_desc")?.ToString()?.Trim() ?? "";
					byPair[$"{baseCur}|||{desc}"] = td;
					if (!byBase.ContainsKey(baseCur)) byBase[baseCur] = td;
				}
			}

			// ---- Merge dữ liệu ----
			var rows = new List<StablecoinItem>();
			foreach (var d in baseRows)
			{
				try
				{
					var root = d as IDictionary<string, object> ?? new Dictionary<string, object>();
					var info = root.TryGetValue("info", out var infoObj) && infoObj is IDictionary<string, object> inf ? inf : new Dictionary<string, object>();
					var market = root.TryGetValue("market", out var mObj) && mObj is IDictionary<string, object> mk ? mk : new Dictionary<string, object>();
					var valuation = root.TryGetValue("valuation", out var vObj) && vObj is IDictionary<string, object> val ? val : new Dictionary<string, object>();

					var baseCur = info.GetValueOrDefault("base_currency")?.ToString()?.ToUpper() ?? "";
					var desc = info.GetValueOrDefault("base_currency_desc")?.ToString()?.Trim() ?? "";
					byPair.TryGetValue($"{baseCur}|||{desc}", out var extra);
					if (extra == null) byBase.TryGetValue(baseCur, out extra);
					extra ??= new Dictionary<string, object>();

					var best = BestYieldHelper.GetBestYield(baseCur);

					double price = ToDouble(market.GetValueOrDefault("close"));
					double maxSupply = ToDouble(info.GetValueOrDefault("max_supply"));
					double totalSupply = ToDouble(info.GetValueOrDefault("total_supply"));
					double? fdv = null;
					if (price > 0)
						fdv = maxSupply > 0 ? price * maxSupply : totalSupply > 0 ? price * totalSupply : null;

					string? logoid = info.GetValueOrDefault("base_currency_logoid")?.ToString();
					double vol24 = ToDouble(market.GetValueOrDefault("24h_vol_cmc") ?? extra.GetValueOrDefault("24h_vol_cmc") ?? info.GetValueOrDefault("24h_vol_cmc"));

					var item = new StablecoinItem
					{
						Symbol = root.GetValueOrDefault("symbol")?.ToString(),
						Currency = baseCur,
						Name = desc ?? baseCur ?? root.GetValueOrDefault("symbol")?.ToString(),
						Logo = !string.IsNullOrEmpty(logoid) ? $"https://s3-symbol-logo.tradingview.com/{logoid}.svg" : null,
						Price = price,
						MarketCap = ToDouble(valuation.GetValueOrDefault("market_cap_calc") ?? extra.GetValueOrDefault("market_cap_calc")),
						Rank = ToInt(valuation.GetValueOrDefault("crypto_total_rank") ?? root.GetValueOrDefault("crypto_total_rank")),
						Categories = (info.GetValueOrDefault("crypto_common_categories_tr") as IEnumerable<object>)?.Select(x => x.ToString() ?? "").ToList(),
						Close = price,
						Mechanism = extra.GetValueOrDefault("mechanism")?.ToString(),
						DepeggingHistory = ToDouble(extra.GetValueOrDefault("depegging_history")),
						Vol24h = vol24,
						BriefInfo = extra.GetValueOrDefault("brief_info")?.ToString(),
						CirculatingSupply = ToDouble(info.GetValueOrDefault("circulating_supply")),
						TotalSupply = ToDouble(info.GetValueOrDefault("total_supply")),
						MaxSupply = ToDouble(info.GetValueOrDefault("max_supply")),
						Fdv = fdv,
						BestYield = best != null ? new BestYieldData
						{
							Apy = best.Apy,
							TvlUsd = best.TvlUsd,
							Project = best.Project,
							Chain = best.Chain,
							Pool = best.Pool
						} : null
					};

					rows.Add(item);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[FindStablecoinsAsync] Skip invalid doc: {ex.Message}");
				}
			}

			// ---- Sort theo marketCap ----
			rows = rows.OrderByDescending(r => r.MarketCap ?? 0).ToList();

			return (rows, total);
		}

		// ========== Helper ToDouble ==========
		private static double ToDouble(object? value)
		{
			if (value == null) return 0;
			return value switch
			{
				double d => d,
				float f => f,
				decimal dm => (double)dm,
				long l => l,
				int i => i,
				_ => double.TryParse(value.ToString(), out var parsed) ? parsed : 0
			};
		}

		private static int ToInt(object? value)
		{
			if (value == null) return 0;
			if (value is int i) return i;
			if (int.TryParse(value.ToString(), out var parsed)) return parsed;
			return 0;
		}
	}
}