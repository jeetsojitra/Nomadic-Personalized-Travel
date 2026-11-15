using System;
using System.ComponentModel.DataAnnotations;

namespace Nomadic.Models
{
    public class PaymentViewModel
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public decimal PackagePrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? Duration { get; set; }

        [Required(ErrorMessage = "Please select a travel date")]
        [DataType(DataType.Date)]
        public DateTime TravelDate { get; set; } = DateTime.Today.AddDays(14);

        [Required(ErrorMessage = "Please specify number of travelers")]
        [Range(1, 20, ErrorMessage = "Number of travelers must be between 1 and 20")]
        public int TravelersCount { get; set; } = 2;

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "City / Address is required")]
        public string CustomerAddress { get; set; } = string.Empty;

        public string? SpecialRequests { get; set; }

        [Required(ErrorMessage = "Please select a payment method")]
        public string PaymentMethod { get; set; } = "Credit / Debit Card";

        
        public string? CardNumber { get; set; }
        public string? CardHolder { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCvv { get; set; }

        
        public string? UpiId { get; set; } = "nomadic.travel@icici";
        public string? UpiApp { get; set; } = "Google Pay";

        
        public string? SelectedBank { get; set; } = "HDFC Bank";
        public string? BankAccountType { get; set; } = "Retail";

        
        public decimal WalletBalance { get; set; } = 150000m;
        public bool UseWalletBalance { get; set; } = true;

        
        public decimal Subtotal => PackagePrice * (TravelersCount > 0 ? TravelersCount : 1);
        public decimal TaxesAndFees => Math.Round(Subtotal * 0.05m, 2);
        public decimal TotalAmount => Subtotal + TaxesAndFees;
    }
}
