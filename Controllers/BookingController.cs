using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nomadic.Models;
using Nomadic.Services;

namespace Nomadic.Controllers
{
    public class BookingController : Controller
    {
        private readonly ITravelDataService _travelDataService;

        public BookingController(ITravelDataService travelDataService)
        {
            _travelDataService = travelDataService;
        }

        [HttpGet]
        public IActionResult Payment(int packageId, DateTime? travelDate, int? travelers)
        {
            var package = _travelDataService.GetPackageById(packageId)
                          ?? _travelDataService.GetAllPackages().FirstOrDefault();

            if (package == null)
            {
                return RedirectToAction("Index", "Packages");
            }

            var model = new PaymentViewModel
            {
                PackageId = package.Id,
                PackageName = package.Name,
                Destination = package.Destination,
                PackagePrice = package.Price,
                ImageUrl = package.ImageUrl,
                Duration = package.Duration,
                TravelDate = travelDate ?? DateTime.Today.AddDays(14),
                TravelersCount = travelers.HasValue && travelers.Value > 0 ? travelers.Value : 2,
                PaymentMethod = "Credit / Debit Card"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm(PaymentViewModel model)
        {
            var package = _travelDataService.GetPackageById(model.PackageId);
            if (package == null)
            {
                ModelState.AddModelError(nameof(model.PackageId), "The selected travel package is unavailable.");
                return RedirectToAction("Index", "Packages");
            }
            if (package != null)
            {
                
                model.PackageName = package.Name;
                model.Destination = package.Destination;
                model.PackagePrice = package.Price;
                model.ImageUrl = package.ImageUrl;
                model.Duration = package.Duration;
            }

            if (!ModelState.IsValid)
            {
                return View("Payment", model);
            }

            
            var formattedPaymentMethod = model.PaymentMethod;
            if (model.PaymentMethod == "Net Banking")
            {
                formattedPaymentMethod = string.IsNullOrWhiteSpace(model.SelectedBank)
                    ? "Net Banking"
                    : $"Net Banking ({model.SelectedBank})";
            }
            else if (model.PaymentMethod.Contains("UPI") || model.PaymentMethod.Contains("QR"))
            {
                formattedPaymentMethod = string.IsNullOrWhiteSpace(model.UpiApp)
                    ? "UPI / Instant QR Code"
                    : $"UPI / QR Code ({model.UpiApp})";
            }
            else if (model.PaymentMethod.Contains("Wallet") || model.PaymentMethod.Contains("Valet"))
            {
                formattedPaymentMethod = "Nomadic Travel Valet & Wallet (VIP Credits)";
            }

            var booking = new Booking
            {
                BookingId = string.Empty,
                PackageId = model.PackageId,
                PackageName = model.PackageName,
                Destination = model.Destination,
                PackagePrice = model.PackagePrice,
                TravelDate = model.TravelDate,
                TravelersCount = model.TravelersCount,
                Subtotal = model.Subtotal,
                TaxesAndFees = model.TaxesAndFees,
                TotalAmount = model.TotalAmount,
                CustomerName = model.CustomerName.Trim(),
                CustomerEmail = model.CustomerEmail.Trim(),
                CustomerPhone = model.CustomerPhone.Trim(),
                CustomerAddress = model.CustomerAddress.Trim(),
                SpecialRequests = model.SpecialRequests?.Trim(),
                PaymentMethod = formattedPaymentMethod,
                BookingDate = DateTime.UtcNow,
                Status = "Pending Payment (Demo)"
            };

            _travelDataService.SaveBooking(booking);

            return RedirectToAction(nameof(Confirmation), new { bookingId = booking.BookingId });
        }

        [HttpGet]
        public IActionResult Confirmation(string bookingId)
        {
            if (string.IsNullOrWhiteSpace(bookingId))
            {
                return RedirectToAction("Index", "Home");
            }

            var booking = _travelDataService.GetBookingById(bookingId);
            if (booking == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(booking);
        }
    }
}
