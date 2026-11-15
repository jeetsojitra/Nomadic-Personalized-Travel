using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Nomadic.Services;

namespace Nomadic.ApiControllers;

[ApiController]
[Route("api")]
public class TravelApiController : ControllerBase
{
    private readonly ITravelDataService _travel;
    public TravelApiController(ITravelDataService travel) => _travel = travel;

    [HttpGet("destinations")]
    public IActionResult Destinations() => Ok(_travel.GetAllDestinations());

    [HttpGet("destinations/{id:int}")]
    public IActionResult Destination(int id)
    {
        var item = _travel.GetDestinationById(id);
        return item is null ? NotFound(new { message = "Destination not found." }) : Ok(item);
    }

    [HttpGet("packages")]
    public IActionResult Packages([FromQuery] string? destination = null) => Ok(_travel.GetPackagesByDestination(destination));

    [HttpGet("packages/{id:int}")]
    public IActionResult Package(int id)
    {
        var item = _travel.GetPackageById(id);
        return item is null ? NotFound(new { message = "Package not found." }) : Ok(item);
    }


    [Authorize]
    [HttpGet("bookings/{bookingId}")]
    public IActionResult Booking(string bookingId)
    {
        var booking = _travel.GetBookingById(bookingId);
        if (booking is null) return NotFound(new { message = "Booking not found." });
        if (!string.Equals(booking.CustomerEmail, User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
            return Forbid();
        return Ok(booking);
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", application = "Nomadic", utc = DateTime.UtcNow });
}
