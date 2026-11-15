namespace Nomadic.Models
{
    public class Package
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
        public string FormattedPrice => $"₹{Price:N0}";
        public string ImageUrl { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public List<ItineraryDay> Itinerary { get; set; } = new();
        public List<string> Inclusions { get; set; } = new();
        public List<string> Exclusions { get; set; } = new();
    }
}
