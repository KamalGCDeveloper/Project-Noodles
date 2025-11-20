namespace Noodle.Api.Models
{
    public class ActiveUsersStockChange
    {
        public double Absolute { get; set; }
        public double Percentage { get; set; }
        public string Direction { get; set; } = "no-change";
    }

    public class ActiveUsersStock
    {
        public double Value { get; set; }
        public ActiveUsersStockChange Change { get; set; } = new();
    }
}