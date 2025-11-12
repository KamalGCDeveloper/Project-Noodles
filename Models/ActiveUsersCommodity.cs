namespace Noodle.Api.Models
{
    public class ActiveUsersChangeData
    {
        public double Absolute { get; set; }
        public double Percentage { get; set; }
        public string Direction { get; set; } = "no-change";
    }

    public class ActiveUsersCommodity
    {
        public int Value { get; set; }
        public ActiveUsersChangeData Change { get; set; } = new ActiveUsersChangeData();
    }
}