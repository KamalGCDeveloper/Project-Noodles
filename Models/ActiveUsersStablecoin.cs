namespace Noodle.Api.Models
{
    public class ActiveUsersChange
    {
        public double Absolute { get; set; }
        public double Percentage { get; set; }
        public string Direction { get; set; } = string.Empty;
    }

    public class ActiveUsersStablecoin
    {
        public double Value { get; set; }
        public ActiveUsersChange Change { get; set; } = new();
    }
}