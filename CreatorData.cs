using System.Reflection.Metadata;

namespace CreatorCoffinTracker
{
    public class CreatorData
    {
        public double Day { get; set; } = 0;
        public string? Code { get; set; }
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }
        public double Players { get; set; } = 0;
        public byte[] AvatarData { get; set; } = [];
        public Channel? Channel { get; set; }
        public List<Days> Days { get; set; } = [];
        public int StatusCode { get; set; }
    }

    public class Channel
    {
        public string? Platform { get; set; }
        public string? Handle { get; set; }
        public string? URL { get; set; }
    }

    public class Days
    {
        public string? DayName { get; set; }
        public string? TrackerID { get; set; }
        public double Value { get; set; } = 0;
        public List<double> Milestones { get; set; } = [];
    }
}
