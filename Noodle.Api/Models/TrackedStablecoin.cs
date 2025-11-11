namespace Noodle.Api.Models
{
    public class TrackedStablecoinChange
    {
        public double Absolute { get; set; }
        public double Percentage { get; set; }
        public string Direction { get; set; } = string.Empty;
    }

    public class TrackedStablecoin
    {
        public int Value { get; set; }
        public TrackedStablecoinChange Change { get; set; } = new();
    }
}