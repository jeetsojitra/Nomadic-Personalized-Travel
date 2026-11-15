namespace Nomadic.Models
{
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public string FormattedPrice => $"₹{StartingPrice:N0}";
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = "Indian"; 
        public string Duration { get; set; } = "4-6 Days";
    }
}
