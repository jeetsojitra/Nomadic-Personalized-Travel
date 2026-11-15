namespace Nomadic.Models
{
    public class ItineraryDay
    {
        public int DayNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Meals { get; set; }
        public string? ActivityHighlight { get; set; }
        public string? Accommodation { get; set; } 
    }
}
