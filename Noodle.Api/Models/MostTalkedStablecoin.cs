using MongoDB.Bson.Serialization.Attributes;

namespace Noodle.Api.Models
{
    public class MostTalkedStablecoin
    {
        [BsonElement("rank")]
        public int Rank { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [BsonElement("mentions")]
        public double Mentions { get; set; }

        [BsonElement("logo")]
        public string Logo { get; set; } = string.Empty;
    }
}