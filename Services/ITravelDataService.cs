using System.Collections.Generic;
using Nomadic.Models;

namespace Nomadic.Services
{
    public interface ITravelDataService
    {
        IReadOnlyList<Destination> GetAllDestinations();
        Destination? GetDestinationById(int id);
        Destination? GetDestinationByName(string name);

        IReadOnlyList<Package> GetAllPackages();
        IReadOnlyList<Package> GetPackagesByDestination(string? destination);
        Package? GetPackageById(int id);

        Booking SaveBooking(Booking booking);
        Booking? GetBookingById(string bookingId);
        IReadOnlyList<Booking> GetAllBookings();
    }
}
