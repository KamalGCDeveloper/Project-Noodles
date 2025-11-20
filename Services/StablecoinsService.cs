using Noodle.Api.Models;
using Noodle.Api.Repositories;
using Noodle.Api.Helpers;

namespace Noodle.Api.Services
{
    public interface IStablecoinsService
    {
        Task<List<TopGrowthStablecoin>> GetTopGrowthStablecoinsAsync();
        Task<List<MostTalkedStablecoin>> GetMostTalkedStablecoinsAsync();
        Task<TrackedStablecoin> GetNumberTrackedStablecoinsAsync();
        Task<ActiveUsersStablecoin> GetActiveUsersStablecoinsAsync();
        Task<StablecoinListResponse> GetStablecoinsAsync(string? q, int page, int limit);
    }

    public class StablecoinsService : IStablecoinsService
    {
        private readonly IStablecoinsRepository _repository;

        public StablecoinsService(IStablecoinsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TopGrowthStablecoin>> GetTopGrowthStablecoinsAsync()
        {
            var stablecoins = await _repository.FindTopStablecoinsByMentionGrowthAsync();
            return stablecoins;
        }

        public async Task<List<MostTalkedStablecoin>> GetMostTalkedStablecoinsAsync()
        {
            var stablecoins = await _repository.FindTopStablecoinsByMentionsAsync();
            return stablecoins;
        }

        public async Task<TrackedStablecoin> GetNumberTrackedStablecoinsAsync()
        {
            var (total, newCount7d, growth7dPercent) = await _repository.GetStablecoinCountAndGrowthAsync();

            var changeDirection = growth7dPercent > 0 ? "up" : "down";

            return new TrackedStablecoin
            {
                Value = total,
                Change = new TrackedStablecoinChange
                {
                    Absolute = newCount7d,
                    Percentage = growth7dPercent,
                    Direction = changeDirection
                }
            };
        }

        public async Task<ActiveUsersStablecoin> GetActiveUsersStablecoinsAsync()
        {
            var (total, last7days, percent) = await _repository.CountAndRecentUsersStablecoinsAsync();

            string direction = percent == 0 ? "no-change" : percent > 0 ? "up" : "down";

            return new ActiveUsersStablecoin
            {
                Value = total,
                Change = new ActiveUsersStablecoinChange
                {
                    Absolute = last7days,
                    Percentage = percent,
                    Direction = direction
                }
            };
        }

        public async Task<StablecoinListResponse> GetStablecoinsAsync(string? q, int page, int limit)
        {
            int skip = (page - 1) * limit;
            var (rows, total) = await _repository.FindStablecoinsAsync(q, skip, limit);

            return new StablecoinListResponse
            {
                Items = rows,
                Page = page,
                Limit = limit,
                Total = total
            };
        }
    }
}