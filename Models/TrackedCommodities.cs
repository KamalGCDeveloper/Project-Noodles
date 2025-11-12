namespace Noodle.Api.Models
{
    public class ChangeData
    {
        public double Absolute { get; set; }
        public double Percentage { get; set; }
        public string Direction { get; set; } = "no-change";
    }

    public class TrackedCommodities
    {
        public int Value { get; set; }
        public ChangeData Change { get; set; } = new ChangeData();
    }
}