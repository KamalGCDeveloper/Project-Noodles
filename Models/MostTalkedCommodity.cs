namespace Noodle.Api.Models
{
    public class MostTalkedCommodity
    {
        public int Rank { get; set; }
        public string? Name { get; set; }
        public string? NameSlug { get; set; }
        public string? Group { get; set; }
        public string? Symbol { get; set; }
        public string? MediumLogoUrl { get; set; }
        public string? Exchange { get; set; }
        public int Mentions { get; set; }
    }
}