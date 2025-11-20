using MongoDB.Driver;
using Noodle.Api.Models;
using Noodle.Api.Data;
using Noodle.Api.Helpers;
using MongoDB.Bson;

namespace Noodle.Api.Repositories
{
	public interface IStocksRepository
	{
		Task<List<TopGrowthStock>> GetTopGrowthStocksAsync();
		Task<List<MostTalkedStock>> GetMostTalkedAboutStocksAsync();
		Task<(int Count, int NewCount7d, double Growth7dPercent)> GetCountStocksAndGrowthAsync();
		Task<(double Total, double Last7days, double Percent)> CountAllAndRecentUsersAsync();
		Task<List<StockItem>> GetAllStocksAsync(string? search, string? groupFilter);
	}

	public class StocksRepository : IStocksRepository
	{
		private readonly IMongoCollection<dynamic> _collection;

		public StocksRepository(IMongoClient client, IConfiguration config)
		{
			var db = client.GetDatabase(config["DatabaseSettings:DatabaseName"]);
			_collection = db.GetCollection<dynamic>("stocks");
		}

		public async Task<List<TopGrowthStock>> GetTopGrowthStocksAsync()
		{
			var builder = Builders<dynamic>.Filter;
			var baseFilter = builder.And(
					builder.Eq("marketType", "usa"),
					builder.Eq("type", "stock"),
					builder.Gt("change_1w", -90),
					builder.Lt("change_1w", 500),
					builder.Gt("close", 0.0001)
			);

			var projection = Builders<dynamic>.Projection
					.Include("logoid")
					.Include("close")
					.Include("description")
					.Include("change_1w")
					.Include("market_cap_basic")
					.Include("name")
					.Include("symbol");

			var docs = await _collection
					.Find(baseFilter)
					.Project<dynamic>(projection)
					.Sort(Builders<dynamic>.Sort.Descending("market_cap_basic"))
					.Limit(5)
					.ToListAsync();

			var result = new List<TopGrowthStock>();
			int rank = 1;

			foreach (var d in docs)
			{
				if (d is IDictionary<string, object> stock)
				{
					var logoid = stock.GetValueOrDefault("logoid")?.ToString();
					var logo = !string.IsNullOrWhiteSpace(logoid)
							? $"https://s3-symbol-logo.tradingview.com/{logoid}.svg"
							: null;

					double change1w = ToDouble(stock.GetValueOrDefault("change_1w"));
					double close = ToDouble(stock.GetValueOrDefault("close"));

					result.Add(new TopGrowthStock
					{
						Rank = rank++,
						Name = stock.GetValueOrDefault("name")?.ToString(),
						Description = stock.GetValueOrDefault("description")?.ToString(),
						Symbol = stock.GetValueOrDefault("symbol")?.ToString(),
						Logo = logo,
						Change1w = change1w,
						Close = close,
						GrowthRate7d = change1w
					});
				}
			}

			return result;
		}

		public async Task<List<MostTalkedStock>> GetMostTalkedAboutStocksAsync()
		{
			var db = _collection.Database;
			var videosCol = db.GetCollection<dynamic>("v4_youtube_videos");

			// --- Lấy danh sách stocks ---
			var stocks = await GetAllStocksAsync();
			var symbols = stocks.Select(s => s.Symbol).Where(s => !string.IsNullOrEmpty(s)).ToList();

			if (symbols.Count == 0) return new List<MostTalkedStock>();

			// --- Pipeline tính mentions ---
			var pipeline = new[]
			{
				new BsonDocument("$match", new BsonDocument("hashtag", new BsonDocument("$in", new BsonArray(symbols)))),
				new BsonDocument("$group", new BsonDocument
					{
						{ "_id", "$hashtag" },
						{ "mentions", new BsonDocument("$sum", 1) }
					})
			};

			var counts = await videosCol.Aggregate<BsonDocument>(pipeline).ToListAsync();

			var mentionsMap = counts.ToDictionary(
					d => d["_id"].AsString,
					d => d["mentions"].AsInt32
			);

			// --- Merge & sort ---
			var result = stocks
					.Select((s, idx) =>
					{
						var symbol = s.Symbol ?? "";
						var mentions = mentionsMap.ContainsKey(symbol) ? mentionsMap[symbol] : 0;
						return new MostTalkedStock
						{
							Rank = 0,
							Name = s.Name,
							Description = s.Description,
							Symbol = symbol,
							Mentions = mentions,
							Logo = !string.IsNullOrEmpty(s.Logo)
													? s.Logo
													: !string.IsNullOrEmpty(s.Logoid)
															? $"https://s3-symbol-logo.tradingview.com/{s.Logoid}.svg"
															: null
						};
					})
					.OrderByDescending(x => x.Mentions)
					.Take(5)
					.ToList();

			// Gán rank
			for (int i = 0; i < result.Count; i++)
				result[i].Rank = i + 1;

			return result;
		}

		public async Task<List<(string? Symbol, string? Name, string? Description, string? Logoid, string? Logo)>> GetAllStocksAsync()
		{
			var builder = Builders<dynamic>.Filter;
			var filter = builder.And(
					builder.Eq("marketType", "usa"),
					builder.Eq("type", "stock"),
					builder.Exists("social", true)
			);

			var projection = Builders<dynamic>.Projection
					.Include("symbol")
					.Include("name")
					.Include("description")
					.Include("logoid");

			var docs = await _collection.Find(filter).Project<dynamic>(projection).Limit(100).ToListAsync();

			var list = new List<(string?, string?, string?, string?, string?)>();
			foreach (var d in docs)
			{
				if (d is IDictionary<string, object> stock)
				{
					var logoid = stock.GetValueOrDefault("logoid")?.ToString();
					list.Add((
							stock.GetValueOrDefault("symbol")?.ToString(),
							stock.GetValueOrDefault("name")?.ToString(),
							stock.GetValueOrDefault("description")?.ToString(),
							logoid,
							!string.IsNullOrEmpty(logoid) ? $"https://s3-symbol-logo.tradingview.com/{logoid}.svg" : null
					));
				}
			}

			return list;
		}

		public async Task<(int Count, int NewCount7d, double Growth7dPercent)> GetCountStocksAndGrowthAsync()
		{
			var now = DateTime.UtcNow;
			var stockCollection = _collection;

			// Tổng số stock đã crawl tới hiện tại
			var count = (int)await stockCollection.CountDocumentsAsync(Builders<dynamic>.Filter.Lte("crawledAt", now));

			// Số stock mới trong 7 ngày gần nhất
			var sevenDaysAgo = now.AddDays(-7);
			var filterNew7d = Builders<dynamic>.Filter.And(
					Builders<dynamic>.Filter.Gt("crawledAt", sevenDaysAgo),
					Builders<dynamic>.Filter.Lt("crawledAt", now)
			);

			var newCount7d = (int)await stockCollection.CountDocumentsAsync(filterNew7d);

			// Tính % tăng trưởng
			var count7dAgo = count - newCount7d;
			double growth7dPercent = count7dAgo > 0
					? Math.Round((double)newCount7d / count7dAgo * 100, 2)
					: 100;

			return (count, newCount7d, growth7dPercent);
		}

		public async Task<(double Total, double Last7days, double Percent)> CountAllAndRecentUsersAsync()
		{
			var db = _collection.Database;
			var userCollection = db.GetCollection<BsonDocument>("stock_x_users");

			var now = DateTime.UtcNow;
			var sevenDaysAgo = now.AddDays(-7);

			var pipeline = new[]
			{
				new BsonDocument("$facet", new BsonDocument
				{
						{ "total", new BsonArray { new BsonDocument("$count", "count") } },
						{ "last7days", new BsonArray {
								new BsonDocument("$match", new BsonDocument("lastUpdated",
										new BsonDocument("$gte", sevenDaysAgo))),
								new BsonDocument("$count", "count")
						} }
				}),
				new BsonDocument("$project", new BsonDocument
				{
						{ "total", new BsonDocument("$ifNull", new BsonArray {
								new BsonDocument("$arrayElemAt", new BsonArray { "$total.count", 0 }), 0
						}) },
						{ "last7days", new BsonDocument("$ifNull", new BsonArray {
								new BsonDocument("$arrayElemAt", new BsonArray { "$last7days.count", 0 }), 0
						}) }
				})
		};

			var result = await userCollection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

			double total = result?["total"].ToDouble() ?? 0;
			double last7days = result?["last7days"].ToDouble() ?? 0;
			double before7days = total - last7days;
			double increase = last7days;
			double percent = before7days <= 0 ? 100 : Math.Round((increase / before7days) * 100, 2);

			return (total, last7days, percent);
		}

		public async Task<List<StockItem>> GetAllStocksAsync(string? search, string? groupFilter)
		{
			var filter = Builders<dynamic>.Filter.Eq("marketType", "usa") &
									 Builders<dynamic>.Filter.Eq("type", "stock") &
									 Builders<dynamic>.Filter.Exists("social");

			if (!string.IsNullOrWhiteSpace(search))
			{
				var regex = new BsonRegularExpression(search.Trim(), "i");
				var searchFilter = Builders<dynamic>.Filter.Or(
						Builders<dynamic>.Filter.Regex("name", regex),
						Builders<dynamic>.Filter.Regex("symbol", regex)
				);
				filter &= searchFilter;
			}

			var sortFieldMap = new Dictionary<string, string>
						{
								{ "valuation", "price_earnings_ttm" },
								{ "performance", "perf_all" },
								{ "dividends", "dividends_yield" },
								{ "profitability", "net_margin_fy" },
								{ "income", "net_income_fy" },
								{ "balance", "total_assets_fq" },
								{ "cashflow", "cash_f_operating_activities_ttm" },
								{ "technicals", "rsi" },
						};

			var sortField = string.IsNullOrEmpty(groupFilter)
					? "market_cap_basic"
					: sortFieldMap.GetValueOrDefault(groupFilter, "market_cap_basic");

			var result = await _collection.Aggregate()
					.Match(filter)
					.Group(new BsonDocument
					{
										{ "_id", "$logoid" },
										{ "doc", new BsonDocument("$first", "$$ROOT") }
					})
					.ReplaceRoot<StockItem>("$doc")
					.Project(new BsonDocument
					{
										{ "_id", 0 },
										{ "symbol", 1 },
										{ "name", 1 },
										{ "logoid", 1 },
										{ "description", 1 },
										{ "market_cap_basic", 1 },
										{ "volume", 1 },
										{ "change", 1 },
										{ "change_1w", 1 },
										{ "relative_volume_10d_calc", 1 },
										{ "earnings_per_share_diluted_ttm", 1 },
										{ "earnings_per_share_diluted_yoy_growth_ttm", 1 },
										{ "dividends_yield", 1 },
										{ "close", 1 },
										{ "price_earnings_ttm", 1 },
					})
					.Sort(Builders<BsonDocument>.Sort.Descending(sortField))
					.Limit(100)
					.ToListAsync();

			return result.Select(r => new StockItem
			{
				Symbol = r.GetValue("symbol", "").AsString,
				Name = r.GetValue("name", "").AsString,
				Logo = $"https://s3-symbol-logo.tradingview.com/{r.GetValue("logoid", "").AsString}.svg",
				Description = r.GetValue("description", "").AsString,

				MarketCapBasic = SafeGetDouble(r, "market_cap_basic"),
				Volume = SafeGetDouble(r, "volume"),
				Change = SafeGetDouble(r, "change"),
				Change1w = SafeGetDouble(r, "change_1w"),
				RelativeVolume10dCalc = SafeGetDouble(r, "relative_volume_10d_calc"),
				EarningsPerShareDilutedTtm = SafeGetDouble(r, "earnings_per_share_diluted_ttm"),
				EarningsPerShareDilutedYoyGrowthTtm = SafeGetDouble(r, "earnings_per_share_diluted_yoy_growth_ttm"),
				DividendsYield = SafeGetDouble(r, "dividends_yield"),
				Close = SafeGetDouble(r, "close"),
				PriceEarningsTtm = SafeGetDouble(r, "price_earnings_ttm")
			}).ToList();
		}

		private static double? SafeGetDouble(BsonDocument doc, string field)
		{
			if (!doc.Contains(field) || doc[field].IsBsonNull) return 0;

			var value = doc[field];
			if (value.IsDouble) return value.AsDouble;
			if (value.IsInt32) return value.AsInt32;
			if (value.IsInt64) return value.AsInt64;
			if (value.IsDecimal128) return (double)value.AsDecimal128;

			return 0;
		}

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
	}
}