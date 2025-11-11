using MongoDB.Bson.Serialization.Attributes;

namespace Noodle.Api.Models
{
    public class TopGrowthStablecoin
    {
        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("symbol")]
        public string Symbol { get; set; } = null!;

        [BsonElement("logo")]
        public string Logo { get; set; } = null!;

        [BsonElement("growthRate7d")]
        public double GrowthRate7d { get; set; }

        [BsonElement("mentions")]
        public double? Mentions { get; set; }

        [BsonElement("mentions_7d")]
        public double? Mentions7d { get; set; }
    }
}