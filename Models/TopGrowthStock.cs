namespace Noodle.Api.Models
{
    public class TopGrowthStock
    {
        public int Rank { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Symbol { get; set; }
        public string? Logo { get; set; }
        public double? Change1w { get; set; }
        public double? Close { get; set; }
        public double? GrowthRate7d { get; set; }
    }
}