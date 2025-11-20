using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Noodle.Api.Models;

namespace Noodle.Api.Repositories
{
    public interface IPriceHistoryRepository
    {
        Task<CryptoToken?> GetCrypto(string symbol);
        Task<Stock?> GetStock(string symbol);
        Task<Commodity?> GetCommodity(string slug);
    }

    public class PriceHistoryRepository : IPriceHistoryRepository
    {
        private readonly IMongoCollection<CryptoToken> _tokenCollection;
        private readonly IMongoCollection<Stock> _stocksCollection;
        private readonly IMongoCollection<Commodity> _commoditiesCollection;

        public PriceHistoryRepository(IMongoClient client, IConfiguration config)
        {
            var db = client.GetDatabase(config["DatabaseSettings:DatabaseName"]);

            _tokenCollection = db.GetCollection<CryptoToken>("v2-token");
            _stocksCollection = db.GetCollection<Stock>("stocks");
            _commoditiesCollection = db.GetCollection<Commodity>("commodities");
        }

        public Task<CryptoToken?> GetCrypto(string symbol)
        {
            return _tokenCollection
                .Find(x => x.Symbol == symbol)
                .FirstOrDefaultAsync();
        }

        public Task<Stock?> GetStock(string symbol)
        {
            return _stocksCollection
                .Find(x =>
                    x.Symbol == symbol &&
                    x.Type == "stock" &&
                    x.MarketType == "usa"
                )
                .FirstOrDefaultAsync();
        }

        public Task<Commodity?> GetCommodity(string slug)
        {
            return _commoditiesCollection
                .Find(x => x.NameSlug == slug)
                .FirstOrDefaultAsync();
        }
    }
}