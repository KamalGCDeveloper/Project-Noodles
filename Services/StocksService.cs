using Noodle.Api.Models;
using Noodle.Api.Repositories;

namespace Noodle.Api.Services
{
	public interface IStocksService
	{
		Task<List<TopGrowthStock>> GetTopGrowthStocksAsync();
		Task<List<MostTalkedStock>> GetMostTalkedAboutStocksAsync();
		Task<TrackedStock> GetNumberTrackedAsync();
		Task<ActiveUsersStock> GetTotalActiveUsers7dAsync();
		Task<StockList> GetStockHealthRanksAsync(int limit = 25, int page = 1, string? search = null, string? groupFilter = null);
	}

	public class StocksService : IStocksService
	{
		private readonly IStocksRepository _repository;

		public StocksService(IStocksRepository repository)
		{
			_repository = repository;
		}

		public async Task<List<TopGrowthStock>> GetTopGrowthStocksAsync()
		{
			return await _repository.GetTopGrowthStocksAsync();
		}

		public async Task<List<MostTalkedStock>> GetMostTalkedAboutStocksAsync()
		{
			return await _repository.GetMostTalkedAboutStocksAsync();
		}

		public async Task<TrackedStock> GetNumberTrackedAsync()
		{
			var (count, newCount7d, growth7dPercent) = await _repository.GetCountStocksAndGrowthAsync();

			var direction = growth7dPercent == 0 ? "no-change" :
											growth7dPercent > 0 ? "up" : "down";

			return new TrackedStock
			{
				Value = count,
				Change = new TrackedStockChange
				{
					Absolute = newCount7d,
					Percentage = growth7dPercent,
					Direction = direction
				}
			};
		}


		public async Task<ActiveUsersStock> GetTotalActiveUsers7dAsync()
		{
			var (total, last7days, percent) = await _repository.CountAllAndRecentUsersAsync();

			var direction = percent == 0 ? "no-change" : percent > 0 ? "up" : "down";

			return new ActiveUsersStock
			{
				Value = total,
				Change = new ActiveUsersStockChange
				{
					Absolute = last7days,
					Percentage = percent,
					Direction = direction
				}
			};
		}

		public async Task<StockList> GetStockHealthRanksAsync(int limit = 25, int page = 1, string? search = null, string? groupFilter = null)
		{
			var allStocks = await _repository.GetAllStocksAsync(search, groupFilter);
			var totalItems = allStocks.Count;
			var totalPages = (int)Math.Ceiling((double)totalItems / limit);
			var start = (page - 1) * limit;

			var paginatedItems = allStocks
				.Skip(start)
				.Take(limit)
				.Select((item, index) =>
				{
					item.Rank = start + index + 1;
					return item;
				})
				.ToList();

			return new StockList
			{
				Items = paginatedItems,
				Metadata = new StockMetadata
				{
					Pagination = new StockPagination
					{
						CurrentPage = page,
						TotalPages = totalPages,
						ItemsPerPage = limit,
						TotalItems = totalItems
					},
					Timestamp = DateTime.UtcNow.ToString("o"),
					Source = "dashboard-analytics",
					TrendChartTimespan = "7d",
					TrendChartUnit = "days"
				}
			};
		}
	}
}