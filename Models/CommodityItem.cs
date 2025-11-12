using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Noodle.Api.Models
{
    [BsonIgnoreExtraElements]
    public class CommodityItem
    {
        [BsonElement("ytd")]
        public string? Ytd { get; set; }

        [BsonElement("yoy")]
        public string? Yoy { get; set; }

        [BsonElement("weekly")]
        public string? Weekly { get; set; }

        [BsonElement("monthly")]
        public string? Monthly { get; set; }

        [BsonElement("unit")]
        public string? Unit { get; set; }

        [BsonElement("trend")]
        public string? Trend { get; set; }

        [BsonElement("price")]
        public string? Price { get; set; }

        [BsonElement("percent")]
        public string? Percent { get; set; }

        [BsonElement("name_slug")]
        public string? NameSlug { get; set; }

        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("group")]
        public string? Group { get; set; }

        [BsonElement("date")]
        public string? Date { get; set; }

        [BsonElement("day")]
        public string? Day { get; set; }

        [BsonElement("energyType")]
        public string EnergyType { get; set; } = ""; // mặc định chuỗi rỗng

        [BsonElement("medium_logo_url")]
        public string? MediumLogoUrl { get; set; }

        [BsonElement("healthScore")]
        public double HealthScore { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CommodityGroupResult
    {
        [BsonElement("group")]
        public string Group { get; set; } = "";

        [BsonElement("topItems")]
        public List<CommodityItem> TopItems { get; set; } = new();
    }

    public class CommoditiesPagination
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
    }

    public class CommoditiesMetadata
    {
        public object? Filters { get; set; }
        public CommoditiesPagination Pagination { get; set; } = new();
        public string Timestamp { get; set; } = "";
        public string Source { get; set; } = "dashboard-analytics";
        public string TrendChartTimespan { get; set; } = "7d";
        public string TrendChartUnit { get; set; } = "day";
    }

    public class CommoditiesListResponse
    {
        public List<CommodityGroupResult> Data { get; set; } = new();
        public CommoditiesMetadata Metadata { get; set; } = new();
    }
}