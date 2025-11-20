namespace Noodle.Api.Models
{
    public class TrackedStockChange
    {
        public double Absolute { get; set; }
        public double Percentage { get; set; }
        public string Direction { get; set; } = "no-change";
    }

    public class TrackedStock
    {
        public int Value { get; set; }
        public TrackedStockChange Change { get; set; } = new();
    }
}