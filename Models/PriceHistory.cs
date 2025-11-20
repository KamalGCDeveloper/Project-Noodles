using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Noodle.Api.Models
{
    public class PriceHistoryRequest
    {
        [Required]
        public string Symbol { get; set; } = string.Empty;

        public string Interval { get; set; } = "1M";

        public string Type { get; set; } = "crypto";
    }

    public class PriceHistoryResponse
    {
        public string Symbol { get; set; } = "";
        public string Type { get; set; } = "";
        public double C { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double O { get; set; }
        public long UnixTime { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CryptoToken
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("symbol")]
        public string? Symbol { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Commodity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name_slug")]
        public string? NameSlug { get; set; }

        public string? Symbol { get; set; }
        public string? Exchange { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Stock
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string? Symbol { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? MarketType { get; set; }
    }
}