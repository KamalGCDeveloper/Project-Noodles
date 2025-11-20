namespace Noodle.Api.Models
{
    public class StablecoinItem
    {
        public string? Symbol { get; set; }
        public string? Name { get; set; }
        public string? Logo { get; set; }
        public double? Price { get; set; }
        public double? MarketCap { get; set; }
        public int? Rank { get; set; }
        public List<string>? Categories { get; set; }
        public double? Close { get; set; }
        public string? Currency { get; set; }
        public string? Mechanism { get; set; }
        public double? DepeggingHistory { get; set; }
        public double? Vol24h { get; set; }
        public BestYieldData? BestYield { get; set; }
        public string? BriefInfo { get; set; }
        public double? CirculatingSupply { get; set; }
        public double? TotalSupply { get; set; }
        public double? MaxSupply { get; set; }
        public double? Fdv { get; set; }
    }

    public class StablecoinListResponse
    {
        public List<StablecoinItem> Items { get; set; } = new();
        public int Page { get; set; }
        public int Limit { get; set; }
        public int? Total { get; set; }
    }

    public class BestYieldData
    {
        public double Apy { get; set; }
        public double TvlUsd { get; set; }
        public string? Project { get; set; }
        public string? Chain { get; set; }
        public string? Pool { get; set; }
    }
}