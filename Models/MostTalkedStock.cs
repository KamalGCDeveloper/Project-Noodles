namespace Noodle.Api.Models
{
    public class MostTalkedStock
    {
        public int Rank { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Symbol { get; set; }
        public int Mentions { get; set; }
        public string? Logo { get; set; }
    }
}