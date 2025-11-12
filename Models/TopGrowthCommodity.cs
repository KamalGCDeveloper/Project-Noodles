namespace Noodle.Api.Models
{
    public class TopGrowthCommodity
    {
        public int Rank { get; set; }
        public string? Name { get; set; }
        public string? Group { get; set; }
        public string? Weekly { get; set; }
        public double? GrowthRate { get; set; }
        public string? Trend { get; set; }
        public string? EnergyType { get; set; }
        public string? NameSlug { get; set; }
        public string? Exchange { get; set; }
        public string? Symbol { get; set; }
        public string? MediumLogoUrl { get; set; }
    }

    public class CommodityDocument
    {
        public string? Name { get; set; }
        public string? Group { get; set; }
        public string? Weekly { get; set; }
        public string? Trend { get; set; }
        public string? NameSlug { get; set; }
        public string? Exchange { get; set; }
        public string? Symbol { get; set; }
        public string? MediumLogoUrl { get; set; }
    }
}