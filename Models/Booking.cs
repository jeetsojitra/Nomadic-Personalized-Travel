using System;

namespace Nomadic.Models
{
    public class Booking
    {
        public string BookingId { get; set; } = string.Empty;
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal PackagePrice { get; set; }
        public DateTime TravelDate { get; set; }
        public int TravelersCount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxesAndFees { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string? SpecialRequests { get; set; }
        public string PaymentMethod { get; set; } = "Credit / Debit Card";
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Confirmed";
    }
}
