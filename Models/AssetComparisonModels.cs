using System.Collections.Generic;

namespace Noodle.Api.Models
{
    public class ComparisonRequest
    {
        public List<string> AssetIds { get; set; } = new();
        public string AssetType { get; set; } = ""; // stablecoin | stock | commodity
        public List<string>? Metrics { get; set; }  // marketCap, growth, volume...

        public bool IsValidType()
        {
            var t = AssetType.ToLower();
            return t == "stablecoin" || t == "stock" || t == "commodity";
        }
    }

    public class ComparisonResponse
    {
        public string ComparisonType { get; set; } = "";
        public List<ComparisonAsset> Assets { get; set; } = new();
        public ComparisonSummary Summary { get; set; } = new();
    }

    public class ComparisonAsset
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public AssetMetrics Metrics { get; set; } = new();
    }

    public class AssetMetrics
    {
        public double? MarketCap { get; set; }
        public double? Growth24h { get; set; }
        public double? Volume24h { get; set; }
        public double? Price { get; set; }
        public double? Weekly { get; set; }
        public double? Monthly { get; set; }
        public double? YoY { get; set; }
        public double? YTD { get; set; }
    }

    public class ComparisonSummary
    {
        public string? HighestMarketCap { get; set; }
        public string? LowestMarketCap { get; set; }
        public string? FastestGrowth { get; set; }

        public Dictionary<string, double>? PercentageDifferences { get; set; }
    }
}