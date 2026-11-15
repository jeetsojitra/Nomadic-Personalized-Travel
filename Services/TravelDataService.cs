using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nomadic.Data;
using Nomadic.Models;

namespace Nomadic.Services;

public class TravelDataService : ITravelDataService
{
    private readonly NomadicDbContext _db;
    private readonly List<Destination> _destinations;
    private readonly List<Package> _packages;

    public TravelDataService(NomadicDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        var storedDestinations = _db.TravelCatalog.AsNoTracking()
            .Where(x => x.Category == "Destination").Select(x => x.JsonData).ToList();
        var storedPackages = _db.TravelCatalog.AsNoTracking()
            .Where(x => x.Category == "Package").Select(x => x.JsonData).ToList();

        _destinations = storedDestinations.Count > 0
            ? storedDestinations.Select(x => JsonSerializer.Deserialize<Destination>(x)).Where(x => x != null).Cast<Destination>().ToList()
            : InitializeDestinations();
        _packages = storedPackages.Count > 0
            ? storedPackages.Select(x => JsonSerializer.Deserialize<Package>(x)).Where(x => x != null).Cast<Package>().ToList()
            : InitializePackages();

        if (storedDestinations.Count == 0 || storedPackages.Count == 0)
            SeedTravelCatalog(storedDestinations.Count == 0, storedPackages.Count == 0);

        ImportLegacyBookings(environment);
    }

    public IReadOnlyList<Destination> GetAllDestinations() => _destinations;
    public Destination? GetDestinationById(int id) => _destinations.FirstOrDefault(x => x.Id == id);
    public Destination? GetDestinationByName(string name) => _destinations.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    public IReadOnlyList<Package> GetAllPackages() => _packages;

    public IReadOnlyList<Package> GetPackagesByDestination(string? destination)
    {
        if (string.IsNullOrWhiteSpace(destination)) return _packages;
        var value = destination.Trim();
        return _packages.Where(x => x.Destination.Contains(value, StringComparison.OrdinalIgnoreCase) || x.Name.Contains(value, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public Package? GetPackageById(int id) => _packages.FirstOrDefault(x => x.Id == id);

    public Booking SaveBooking(Booking booking)
    {
        if (string.IsNullOrWhiteSpace(booking.BookingId))
            booking.BookingId = $"NOM-{DateTime.UtcNow:yyyy}-{Guid.NewGuid():N}";
        booking.BookingDate = DateTime.UtcNow;
        _db.Bookings.Add(booking);
        _db.SaveChanges();
        return booking;
    }

    public Booking? GetBookingById(string bookingId) =>
        string.IsNullOrWhiteSpace(bookingId) ? null : _db.Bookings.AsNoTracking().FirstOrDefault(x => x.BookingId == bookingId.Trim());

    public IReadOnlyList<Booking> GetAllBookings() => _db.Bookings.AsNoTracking().OrderByDescending(x => x.BookingDate).ToList();

    private void SeedTravelCatalog(bool seedDestinations, bool seedPackages)
    {
        if (seedDestinations)
        {
            foreach (var item in _destinations)
                _db.TravelCatalog.Add(new TravelCatalogRecord
                {
                    Category = "Destination",
                    RecordKey = item.Id.ToString(),
                    JsonData = JsonSerializer.Serialize(item)
                });
        }
        if (seedPackages)
        {
            foreach (var item in _packages)
                _db.TravelCatalog.Add(new TravelCatalogRecord
                {
                    Category = "Package",
                    RecordKey = item.Id.ToString(),
                    JsonData = JsonSerializer.Serialize(item)
                });
        }
        _db.SaveChanges();
    }

    private void ImportLegacyBookings(IWebHostEnvironment environment)
    {
        var path = Path.Combine(environment.ContentRootPath, "App_Data", "bookings.json");
        if (!File.Exists(path) || _db.Bookings.Any()) return;
        try
        {
            var records = JsonSerializer.Deserialize<List<Booking>>(File.ReadAllText(path)) ?? new();
            foreach (var booking in records.Where(x => !string.IsNullOrWhiteSpace(x.BookingId)))
                if (!_db.Bookings.Any(x => x.BookingId == booking.BookingId)) _db.Bookings.Add(booking);
            _db.SaveChanges();
        }
        catch (JsonException) { }
    }

    private static List<Destination> InitializeDestinations()
    {
        return new List<Destination>
        {
            
            new()
            {
                Id = 1,
                Name = "Goa",
                Category = "Indian",
                Location = "Goa, India",
                ShortDescription = "Sun-kissed golden beaches, vibrant shacks, Portuguese colonial heritage, and thrilling water sports.",
                StartingPrice = 24999,
                ImageUrl = "/Images/goa1.jpg",
                Duration = "4 Days / 3 Nights"
            },
            new()
            {
                Id = 2,
                Name = "Kashmir",
                Category = "Indian",
                Location = "Jammu & Kashmir, India",
                ShortDescription = "Paradise on Earth with serene Dal Lake Shikara rides, snow slopes of Gulmarg, and lush pine valleys.",
                StartingPrice = 32500,
                ImageUrl = "https://images.unsplash.com/photo-1595815771614-ade9d652a65d?auto=format&fit=crop&w=1000&q=80",
                Duration = "5 Days / 4 Nights"
            },
            new()
            {
                Id = 3,
                Name = "Kerala",
                Category = "Indian",
                Location = "Kerala, India",
                ShortDescription = "God's Own Country featuring emerald backwaters in Alleppey, misty tea plantations of Munnar, and Ayurveda.",
                StartingPrice = 28999,
                ImageUrl = "https://images.unsplash.com/photo-1602216056096-3b40cc0c9944?auto=format&fit=crop&w=1000&q=80",
                Duration = "5 Days / 4 Nights"
            },
            new()
            {
                Id = 4,
                Name = "Jaipur",
                Category = "Indian",
                Location = "Rajasthan, India",
                ShortDescription = "The royal Pink City with grand Amber Fort, Hawa Mahal, desert culture, and authentic Rajasthani heritage.",
                StartingPrice = 21500,
                ImageUrl = "https://images.unsplash.com/photo-1477587458883-47145ed94245?auto=format&fit=crop&w=1000&q=80",
                Duration = "4 Days / 3 Nights"
            },
            new()
            {
                Id = 5,
                Name = "Manali",
                Category = "Indian",
                Location = "Himachal Pradesh, India",
                ShortDescription = "Himalayan alpine paradise with Solang snow adventures, Hadimba Devi temple, and riverside cedar trails.",
                StartingPrice = 19999,
                ImageUrl = "https://images.unsplash.com/photo-1626621341517-bbf3d9990a23?auto=format&fit=crop&w=1000&q=80",
                Duration = "5 Days / 4 Nights"
            },
            new()
            {
                Id = 6,
                Name = "Ladakh",
                Category = "Indian",
                Location = "Ladakh, India",
                ShortDescription = "High-altitude desert marvel with azure Pangong Tso, Khardung La pass, and ancient cliffside monasteries.",
                StartingPrice = 39999,
                ImageUrl = "https://images.unsplash.com/photo-1581793745862-99fde7fa73d2?auto=format&fit=crop&w=1000&q=80",
                Duration = "6 Days / 5 Nights"
            },
            new()
            {
                Id = 7,
                Name = "Andaman Islands",
                Category = "Indian",
                Location = "Andaman & Nicobar, India",
                ShortDescription = "Turquoise waters, pristine Radhanagar Beach, vibrant coral reef scuba diving, and historical Cellular Jail.",
                StartingPrice = 36500,
                ImageUrl = "https://images.unsplash.com/photo-1589308078059-be1415eab4c3?auto=format&fit=crop&w=1000&q=80",
                Duration = "5 Days / 4 Nights"
            },
            new()
            {
                Id = 8,
                Name = "Rishikesh",
                Category = "Indian",
                Location = "Uttarakhand, India",
                ShortDescription = "Yoga capital of the world along the holy Ganges, featuring thrilling white-water rafting and peaceful Ganga Aarti.",
                StartingPrice = 16999,
                ImageUrl = "https://images.unsplash.com/photo-1544735716-392fe2489ffa?auto=format&fit=crop&w=1000&q=80",
                Duration = "3 Days / 2 Nights"
            },
            new()
            {
                Id = 9,
                Name = "Varanasi",
                Category = "Indian",
                Location = "Uttar Pradesh, India",
                ShortDescription = "Spiritual epicenter of India with sunrise boat rides on the Ganges, grand Dashashwamedh Aarti, and Sarnath.",
                StartingPrice = 18500,
                ImageUrl = "https://images.unsplash.com/photo-1561359313-0639aad49ca6?auto=format&fit=crop&w=1000&q=80",
                Duration = "3 Days / 2 Nights"
            },
            new()
            {
                Id = 10,
                Name = "Darjeeling",
                Category = "Indian",
                Location = "West Bengal, India",
                ShortDescription = "Queen of the Hills with glowing Kanchenjunga sunrise at Tiger Hill, heritage Toy Train, and lush tea gardens.",
                StartingPrice = 22500,
                ImageUrl = "https://images.unsplash.com/photo-1544634076-a90160ddf44c?auto=format&fit=crop&w=1000&q=80",
                Duration = "4 Days / 3 Nights"
            },

            
            new()
            {
                Id = 11,
                Name = "Bali",
                Category = "International",
                Location = "Bali, Indonesia",
                ShortDescription = "Tropical paradise with sacred sea temples, lush Ubud rice terraces, volcanic sunrises, and surf beaches.",
                StartingPrice = 48500,
                ImageUrl = "/Images/bali.jpg",
                Duration = "6 Days / 5 Nights"
            },
            new()
            {
                Id = 12,
                Name = "Dubai",
                Category = "International",
                Location = "Dubai, United Arab Emirates",
                ShortDescription = "Futuristic skyline with Burj Khalifa, desert 4x4 dune bashing, luxury marina yachts, and vibrant souks.",
                StartingPrice = 58000,
                ImageUrl = "/Images/dubai.jpg",
                Duration = "5 Days / 4 Nights"
            },
            new()
            {
                Id = 13,
                Name = "Switzerland",
                Category = "International",
                Location = "Swiss Alps, Europe",
                ShortDescription = "Postcard-perfect alpine country with Jungfraujoch Top of Europe, scenic glacier trains, and mirror lakes.",
                StartingPrice = 145000,
                ImageUrl = "/Images/switzerland.jpg",
                Duration = "7 Days / 6 Nights"
            },
            new()
            {
                Id = 14,
                Name = "Santorini",
                Category = "International",
                Location = "Cyclades, Greece",
                ShortDescription = "Romantic Greek haven of whitewashed cliffside villas, blue domes, volcanic caldera cruises, and Oia sunsets.",
                StartingPrice = 115000,
                ImageUrl = "/Images/santorini.jpg",
                Duration = "5 Days / 4 Nights"
            },
            new()
            {
                Id = 15,
                Name = "Paris",
                Category = "International",
                Location = "Paris, France",
                ShortDescription = "The City of Light with the Eiffel Tower summit, Louvre museum treasures, Seine river dining, and chic cafes.",
                StartingPrice = 120000,
                ImageUrl = "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?auto=format&fit=crop&w=1000&q=80",
                Duration = "6 Days / 5 Nights"
            },
            new()
            {
                Id = 16,
                Name = "Tokyo",
                Category = "International",
                Location = "Tokyo, Japan",
                ShortDescription = "Ultra-modern Shibuya crossings, ancient Asakusa shrines, Mount Fuji excursions, and culinary perfection.",
                StartingPrice = 135000,
                ImageUrl = "https://images.unsplash.com/photo-1503899036084-c55cdd92da26?auto=format&fit=crop&w=1000&q=80",
                Duration = "6 Days / 5 Nights"
            },
            new()
            {
                Id = 17,
                Name = "Singapore",
                Category = "International",
                Location = "Singapore",
                ShortDescription = "Futuristic Gardens by the Bay Supertrees, Marina Bay Sands Skypark, and Universal Studios Sentosa.",
                StartingPrice = 52000,
                ImageUrl = "https://images.unsplash.com/photo-1525625293386-3f8f99389edd?auto=format&fit=crop&w=1000&q=80",
                Duration = "5 Days / 4 Nights"
            },
            new()
            {
                Id = 18,
                Name = "Thailand",
                Category = "International",
                Location = "Bangkok & Phuket, Thailand",
                ShortDescription = "Gilded Buddhist temples, bustling river markets, crystal waters of Phi Phi islands, and warm Thai smiles.",
                StartingPrice = 38000,
                ImageUrl = "https://images.unsplash.com/photo-1506665531195-3566af2b4dfa?auto=format&fit=crop&w=1000&q=80",
                Duration = "6 Days / 5 Nights"
            },
            new()
            {
                Id = 19,
                Name = "Rome",
                Category = "International",
                Location = "Rome, Italy",
                ShortDescription = "The Eternal City boasting the Colosseum, Vatican museums, Trevi Fountain wishes, and authentic Italian pasta.",
                StartingPrice = 110000,
                ImageUrl = "https://images.unsplash.com/photo-1552832230-c0197dd311b5?auto=format&fit=crop&w=1000&q=80",
                Duration = "5 Days / 4 Nights"
            },
            new()
            {
                Id = 20,
                Name = "El Nido",
                Category = "International",
                Location = "Palawan, Philippines",
                ShortDescription = "Dramatic limestone monoliths, hidden secret lagoons, turquoise waters, and premier island hopping.",
                StartingPrice = 65000,
                ImageUrl = "/Images/elnido.jpg",
                Duration = "5 Days / 4 Nights"
            }
        };
    }

    private static List<Package> InitializePackages()
    {
        return new List<Package>
        {
            
            new()
            {
                Id = 1,
                Name = "Goa Coastal Vibe & Water Sports",
                Destination = "Goa",
                Duration = "4 Days / 3 Nights",
                DurationDays = 4,
                Price = 24999,
                ImageUrl = "/Images/goa1.jpg",
                ShortDescription = "Experience coastal bliss with parasailing, jet-skiing, Fort Aguada, and Portuguese Latin Quarter.",
                Overview = "Unwind on sunny sands, dive into exhilarating water sports, explore historic Old Goa churches, and cruise down the Mandovi River under sunset skies.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Arrival in Goa & North Beach Leisure", Description = "Arrive at Goa Airport and transfer to your coastal resort. Spend a relaxed afternoon exploring Calangute Beach and watch sunset with fresh seafood at Baga.", Meals = "Welcome Drink", ActivityHighlight = "Baga Beach Sunset", Accommodation = "Taj Fort Aguada Resort & Spa (5-Star Heritage Beach Resort)" },
                    new() { DayNumber = 2, Title = "Water Sports Adventure & Fort Aguada", Description = "Thrilling water sports at Anjuna Beach including parasailing, jet ski, and bumper rides. Tour 17th-century Portuguese Fort Aguada and experience nightlife at Tito's Lane.", Meals = "Breakfast", ActivityHighlight = "Parasailing & Jet Ski Combo", Accommodation = "Taj Fort Aguada Resort & Spa (5-Star Heritage Beach Resort)" },
                    new() { DayNumber = 3, Title = "South Goa Heritage & Mandovi River Cruise", Description = "Visit UNESCO World Heritage Basilica of Bom Jesus, Se Cathedral, and colourful Fontainhas Latin Quarter. Enjoy a scenic 1-hour sunset cruise along Mandovi River with Goan folk music.", Meals = "Breakfast", ActivityHighlight = "Mandovi Sunset Cruise", Accommodation = "Taj Fort Aguada Resort & Spa (5-Star Heritage Beach Resort)" },
                    new() { DayNumber = 4, Title = "Flea Market Souvenirs & Departure", Description = "Morning souvenir shopping at local flea markets for spices, cashews, and beachwear. Private transfer to airport/station for departure.", Meals = "Breakfast", ActivityHighlight = "Local Handicraft Shopping", Accommodation = "Check-out & Airport / Railway Station Transfer" }
                },
                Inclusions = new() { "3 Nights 4-Star Beach Resort Stay", "Daily Buffet Breakfast", "Water Sports Combo (Parasailing, Jet Ski, Banana Ride)", "Mandovi River Sunset Cruise Tickets", "All Sightseeing in Private AC Sedan", "Airport / Railway Station Transfers" },
                Exclusions = new() { "Airfare / Train tickets to Goa", "Personal expenses, alcoholic beverages, and tips", "Scuba diving sessions (available as add-on)", "Meals other than specified in itinerary" }
            },

            
            new()
            {
                Id = 2,
                Name = "Magical Kashmir Valley & Gulmarg Escape",
                Destination = "Kashmir",
                Duration = "5 Days / 4 Nights",
                DurationDays = 5,
                Price = 32500,
                ImageUrl = "https://images.unsplash.com/photo-1595815771614-ade9d652a65d?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Stay in a carved Dal Lake houseboat, ride the Gulmarg Gondola, and stroll royal Shalimar gardens.",
                Overview = "Immerse yourself in Paradise on Earth. Experience romantic Shikara rides, majestic snow peaks of Gulmarg, and lush pine valleys along the Lidder River in Pahalgam.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Arrival in Srinagar & Dal Lake Shikara", Description = "Meet and greet at Srinagar Airport. Check in to a traditional carved cedarwood luxury houseboat on Dal Lake. Enjoy a serene 1-hour sunset Shikara ride.", Meals = "Dinner", ActivityHighlight = "Dal Lake Sunset Shikara", Accommodation = "Mascot Luxury Houseboat on Dal Lake (Carved Cedarwood Suite)" },
                    new() { DayNumber = 2, Title = "Mughal Gardens & Historic Old Srinagar", Description = "Explore the Mughal heritage: Nishat Bagh (Garden of Pleasure) and Shalimar Bagh (Abode of Love). Tour Shankaracharya Temple overlooking the valley.", Meals = "Breakfast & Dinner", ActivityHighlight = "Mughal Royal Gardens", Accommodation = "The Lalit Grand Palace Srinagar (5-Star Heritage Hotel)" },
                    new() { DayNumber = 3, Title = "Gulmarg Snow Terraces & Gondola Ride", Description = "Scenic drive to Gulmarg (Meadow of Flowers). Take the world-famous Gulmarg Gondola cable car up to snow terraces with breathtaking Himalayan views.", Meals = "Breakfast & Dinner", ActivityHighlight = "Gulmarg Gondola Cable Car", Accommodation = "The Vintage Gulmarg Hotel (Alpine Ski Resort)" },
                    new() { DayNumber = 4, Title = "Pahalgam Valley of Shepherds & Betaab Valley", Description = "Drive along Lidder River to picturesque Pahalgam. Visit saffron fields in Pampore, ancient Avantipur ruins, and the lush landscapes of Betaab Valley.", Meals = "Breakfast & Dinner", ActivityHighlight = "Betaab Valley Excursion", Accommodation = "Grand Mumtaz Resort Pahalgam (Riverside Deluxe Room)" },
                    new() { DayNumber = 5, Title = "Floating Vegetable Market & Departure", Description = "Early morning visit to Dal Lake's floating vegetable market. Enjoy traditional Kashmiri Kahwa tea before private transfer to Srinagar Airport.", Meals = "Breakfast", ActivityHighlight = "Authentic Kashmiri Kahwa", Accommodation = "Check-out & Srinagar Airport Departure" }
                },
                Inclusions = new() { "1 Night Luxury Houseboat & 3 Nights 4-Star Hotel", "Daily Breakfast & Authentic Kashmiri Dinners", "1-Hour Sunset Shikara Ride on Dal Lake", "All Sightseeing and transfers in private vehicle", "Driver allowances, toll taxes, and parking" },
                Exclusions = new() { "Gulmarg Gondola Phase 2 tickets", "Pony rides and snow bike activities", "Airfare to/from Srinagar", "Lunches and personal shopping" }
            },

            
            new()
            {
                Id = 3,
                Name = "Kerala Backwaters & Munnar Hills Retreat",
                Destination = "Kerala",
                Duration = "5 Days / 4 Nights",
                DurationDays = 5,
                Price = 28999,
                ImageUrl = "https://images.unsplash.com/photo-1602216056096-3b40cc0c9944?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Misty Munnar tea gardens, spice plantations of Thekkady, and a private Alleppey houseboat cruise.",
                Overview = "Discover God's Own Country. Breathe the crisp mountain air of Munnar's rolling tea estates and gently drift through coconut-fringed backwater canals in a private thatched houseboat.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Cochin Arrival & Scenic Drive to Munnar", Description = "Land at Cochin International Airport. Scenic hill drive stopping at Cheeyappara and Valara waterfalls. Check into your misty Munnar mountain resort.", Meals = "Dinner", ActivityHighlight = "Cheeyappara Waterfall", Accommodation = "Windermere Estate Munnar (Colonial Tea Garden Retreat)" },
                    new() { DayNumber = 2, Title = "Munnar Tea Estates & Eravikulam Park", Description = "Visit Eravikulam National Park to spot the Nilgiri Tahr. Tour the Tata Tea Museum with tea-tasting and photograph Mattupetty Dam and Echo Point.", Meals = "Breakfast & Dinner", ActivityHighlight = "Tata Tea Museum & Tasting", Accommodation = "Windermere Estate Munnar (Colonial Tea Garden Retreat)" },
                    new() { DayNumber = 3, Title = "Spice Plantations to Thekkady", Description = "Drive through Cardamom Hills to Thekkady. Walk through aromatic pepper, cardamom, and cinnamon gardens. Evening option for Periyar Lake boat safari.", Meals = "Breakfast & Dinner", ActivityHighlight = "Guided Spice Plantation Walk", Accommodation = "Spice Village - CGH Earth Thekkady (Eco-Luxury Forest Cottage)" },
                    new() { DayNumber = 4, Title = "Alleppey Houseboat Backwater Cruise", Description = "Drive to Alleppey. Check into an authentic thatched Kettuvallam private houseboat. Cruise through palm-fringed canals with traditional freshly prepared Kerala feasts.", Meals = "Breakfast, Lunch & Dinner", ActivityHighlight = "Private Houseboat Experience", Accommodation = "Private Deluxe Thatched Houseboat on Vembanad Lake (Alleppey)" },
                    new() { DayNumber = 5, Title = "Fort Kochi Heritage & Departure", Description = "Disembark houseboat. Visit Fort Kochi's iconic Chinese fishing nets and historic Jew Town before transfer to Cochin Airport for departure.", Meals = "Breakfast", ActivityHighlight = "Historic Chinese Fishing Nets", Accommodation = "Check-out & Cochin International Airport Departure" }
                },
                Inclusions = new() { "2 Nights Munnar, 1 Night Thekkady, 1 Night Private Houseboat", "All meals on Houseboat (Breakfast, Lunch, Dinner)", "Daily Breakfast at Munnar & Thekkady hotels", "Guided Spice Plantation Tour", "Private AC vehicle for all transfers" },
                Exclusions = new() { "Periyar boat safari tickets", "Ayurvedic massage sessions", "Domestic airfare to Cochin", "Personal tips and laundry" }
            },

            
            new()
            {
                Id = 4,
                Name = "Royal Heritage of Jaipur & Amber Fort",
                Destination = "Jaipur",
                Duration = "4 Days / 3 Nights",
                DurationDays = 4,
                Price = 21500,
                ImageUrl = "https://images.unsplash.com/photo-1477587458883-47145ed94245?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Step into royalty with grand Amber Fort, Hawa Mahal, City Palace, and Chokhi Dhani cultural village.",
                Overview = "Relive the majestic Rajputana era in the Pink City. Marvel at grand hilltop fortresses, ornate mirrored palaces, and vibrant Rajasthani folk dances and delicacies.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Pink City Arrival & Chokhi Dhani Night", Description = "Welcome at Jaipur Airport/Station. Check into heritage haveli hotel. Evening visit to Chokhi Dhani village for folk dances, camel rides, and grand Rajasthani thali.", Meals = "Traditional Dinner", ActivityHighlight = "Chokhi Dhani Cultural Show", Accommodation = "Samode Haveli Heritage Hotel (Royal Deluxe Room)" },
                    new() { DayNumber = 2, Title = "Amber Fort & Jal Mahal Photo Stop", Description = "Ascend to magnificent Amber Fort with Sheesh Mahal (hall of mirrors). Stop by water palace Jal Mahal. Tour astronomical observatory Jantar Mantar and royal City Palace.", Meals = "Breakfast", ActivityHighlight = "Sheesh Mahal Mirrored Palace", Accommodation = "Samode Haveli Heritage Hotel (Royal Deluxe Room)" },
                    new() { DayNumber = 3, Title = "Hawa Mahal, Nahargarh Sunset & Bazaars", Description = "Morning photo stop at honeycomb facade of Hawa Mahal. Panoramic sunset over Jaipur from hilltop Nahargarh Fort. Evening shopping in Johari and Bapu Bazaars.", Meals = "Breakfast", ActivityHighlight = "Nahargarh Fort Sunset View", Accommodation = "Samode Haveli Heritage Hotel (Royal Deluxe Room)" },
                    new() { DayNumber = 4, Title = "Albert Hall Museum & Departure", Description = "Visit Albert Hall Museum and decorative Patrika Gate. Souvenir shopping for blue pottery and block prints before transfer to airport.", Meals = "Breakfast", ActivityHighlight = "Patrika Gate Photography", Accommodation = "Check-out & Jaipur Airport / Railway Station Departure" }
                },
                Inclusions = new() { "3 Nights in 4-Star Heritage Haveli Hotel", "Daily Buffet Breakfast & 1 Gala Rajasthani Dinner", "Chokhi Dhani Village Entry & Dining Pass", "All Transfers and Sightseeing by Private AC Sedan", "Approved Government Monument Guide" },
                Exclusions = new() { "Monument entrance and camera fees", "Elephant or Jeep ride at Amber Fort", "Train / Airfare to Jaipur", "Personal expenses" }
            },

            
            new()
            {
                Id = 5,
                Name = "Manali & Solang Valley Adventure",
                Destination = "Manali",
                Duration = "5 Days / 4 Nights",
                DurationDays = 5,
                Price = 19999,
                ImageUrl = "https://images.unsplash.com/photo-1626621341517-bbf3d9990a23?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Solang Valley snow adventures, Atal Tunnel crossing, Hadimba temple, and riverside cedar trails.",
                Overview = "Escape into the Himalayas. Experience thrilling adventure sports in Solang Valley, marvel at mountain panoramas beyond Atal Tunnel, and relax beside the rushing Beas River.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Arrival in Manali & Old Manali Cafes", Description = "Arrive in Manali. Check into your mountain resort with valley views. Walk to historic Hadimba wooden temple and explore riverside Bohemian cafes in Old Manali.", Meals = "Dinner", ActivityHighlight = "Hadimba Devi Cedar Temple", Accommodation = "Span Resort & Spa, Kullu-Manali (5-Star Riverside Cedar Chalet)" },
                    new() { DayNumber = 2, Title = "Solang Valley Thrills & Atal Tunnel", Description = "Full day at Solang Valley for paragliding, zorbing, and quad-biking. Drive through the engineering marvel Atal Tunnel to picturesque Sissu waterfall in Lahaul.", Meals = "Breakfast & Dinner", ActivityHighlight = "Atal Tunnel & Sissu Valley", Accommodation = "Span Resort & Spa, Kullu-Manali (5-Star Riverside Cedar Chalet)" },
                    new() { DayNumber = 3, Title = "Naggar Castle Heritage & Art Gallery", Description = "Excursion to 500-year-old Naggar Castle offering panoramic Kullu valley vistas. Visit the Nicholas Roerich Art Gallery and stroll through fragrant apple orchards.", Meals = "Breakfast & Dinner", ActivityHighlight = "Naggar Castle Heritage", Accommodation = "Span Resort & Spa, Kullu-Manali (5-Star Riverside Cedar Chalet)" },
                    new() { DayNumber = 4, Title = "Vashisht Hot Springs & Mall Road", Description = "Dip in natural healing sulphur hot springs at Vashisht village. Afternoon shopping for warm Kullu shawls, wooden handicrafts, and mountain honey on Mall Road.", Meals = "Breakfast & Dinner", ActivityHighlight = "Natural Sulphur Hot Springs", Accommodation = "Span Resort & Spa, Kullu-Manali (5-Star Riverside Cedar Chalet)" },
                    new() { DayNumber = 5, Title = "Beas River Morning & Departure", Description = "Leisurely breakfast overlooking snow-clad peaks. Morning walk along Beas River before transfer for return journey.", Meals = "Breakfast", ActivityHighlight = "Riverside Mountain Walk", Accommodation = "Check-out & Manali Volvo / Bhuntar Airport Departure" }
                },
                Inclusions = new() { "4 Nights in Premium Mountain View Resort", "Daily Breakfast & Dinner", "Day Excursion to Solang Valley and Atal Tunnel", "Private AC Vehicle for all Transfers & Sightseeing", "All Tolls, Parking, and Driver Allowances" },
                Exclusions = new() { "Paragliding, zorbing, and snow sports fees", "Rohtang Pass permit and local union taxi", "Travel tickets to Manali", "Personal laundry and tips" }
            },

            
            new()
            {
                Id = 6,
                Name = "Mystical Ladakh & Pangong Lake Expedition",
                Destination = "Ladakh",
                Duration = "6 Days / 5 Nights",
                DurationDays = 6,
                Price = 39999,
                ImageUrl = "https://images.unsplash.com/photo-1581793745862-99fde7fa73d2?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Conquer Khardung La, ride double-humped camels in Nubra, and camp beside blue Pangong Lake.",
                Overview = "Embark on an unforgettable Himalayan expedition across high-altitude mountain passes, lunar landscapes, dramatic Buddhist monasteries, and the ever-changing shades of Pangong Tso.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Leh Airport Arrival & Acclimatization", Description = "Fly into Kushok Bakula Rimpochee Airport (11,500 ft). Essential acclimatization rest day. Sunset walk to Shanti Stupa for sweeping views of Leh town.", Meals = "Dinner", ActivityHighlight = "Shanti Stupa Sunset Panorama", Accommodation = "The Grand Dragon Leh (5-Star Luxury Eco-Resort)" },
                    new() { DayNumber = 2, Title = "Sham Valley, Magnetic Hill & Sangam", Description = "Tour Hall of Fame war museum, mysterious Magnetic Hill, Gurudwara Pathar Sahib, and dramatic confluence (Sangam) of Indus and Zanskar rivers.", Meals = "Breakfast & Dinner", ActivityHighlight = "Indus & Zanskar River Confluence", Accommodation = "The Grand Dragon Leh (5-Star Luxury Eco-Resort)" },
                    new() { DayNumber = 3, Title = "Khardung La Pass to Nubra Valley", Description = "Drive over Khardung La (17,582 ft) - world's highest motorable pass. Visit 106-ft Maitreya Buddha at Diskit Monastery and ride Bactrian camels on Hunder Sand Dunes.", Meals = "Breakfast & Dinner", ActivityHighlight = "Khardung La Pass Crossing", Accommodation = "Hunder Desert Luxury Tented Camp, Nubra Valley" },
                    new() { DayNumber = 4, Title = "Nubra Valley to Surreal Pangong Tso", Description = "Scenic drive along the Shyok River gorge to high-altitude Pangong Lake. Check into luxury lakeside tents and marvel at shifting shades of blue.", Meals = "Breakfast & Dinner", ActivityHighlight = "Lakeside Glamping at Pangong", Accommodation = "Pangong Vista Glamping Resort (Luxury Heated Camp on Lake Shore)" },
                    new() { DayNumber = 5, Title = "Pangong Sunrise to Leh via Chang La", Description = "Witness magical sunrise over Pangong Lake. Drive back to Leh crossing Chang La pass (17,590 ft). Evening shopping in Leh Main Bazaar for Pashmina shawls.", Meals = "Breakfast & Dinner", ActivityHighlight = "Pangong Tso Sunrise Magic", Accommodation = "The Grand Dragon Leh (5-Star Luxury Eco-Resort)" },
                    new() { DayNumber = 6, Title = "Farewell Leh & Departure", Description = "Early morning transfer to Leh Airport with cherished memories of the Land of High Passes.", Meals = "Breakfast", ActivityHighlight = "Himalayan Aerial Views", Accommodation = "Check-out & Leh Kushok Bakula Rimpochee Airport Departure" }
                },
                Inclusions = new() { "3 Nights Leh Hotel, 1 Night Nubra Camp, 1 Night Pangong Camp", "Daily Breakfast & Dinner at all accommodations", "Inner Line Permits and Wildlife Conservation Fees", "Private SUV (Innova / Scorpio) with Oxygen Cylinder Support", "All airport and sightseeing transfers" },
                Exclusions = new() { "Bactrian camel ride charges at Hunder", "Monastery entrance and camera fees", "Domestic airfare to/from Leh", "Personal medical expenses" }
            },

            
            new()
            {
                Id = 7,
                Name = "Exotic Andaman & Havelock Island Retreat",
                Destination = "Andaman Islands",
                Duration = "5 Days / 4 Nights",
                DurationDays = 5,
                Price = 36500,
                ImageUrl = "https://images.unsplash.com/photo-1589308078059-be1415eab4c3?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Asia's best Radhanagar Beach, Cellular Jail Light & Sound show, and Elephant Beach coral reefs.",
                Overview = "Escape to emerald islands in the Bay of Bengal. Walk barefoot on powdery white sand, cruise on high-speed catamarans, and snorkel among dazzling living coral gardens.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Port Blair Arrival & Cellular Jail Show", Description = "Arrive at Port Blair Airport. Check into hotel. Visit historic Cellular Jail and attend the emotional patriotic Light & Sound show in the evening.", Meals = "Welcome Drink", ActivityHighlight = "Cellular Jail Light & Sound", Accommodation = "SeaShell Port Blair (4-Star Bay View Resort)" },
                    new() { DayNumber = 2, Title = "Catamaran Cruise to Havelock & Radhanagar", Description = "Board Makruzz luxury catamaran to Havelock Island. Check into beach resort. Spend an idyllic afternoon on Asia's famous Radhanagar Beach watching sunset.", Meals = "Breakfast", ActivityHighlight = "Radhanagar Beach Sunset", Accommodation = "Symphony Palms Beach Resort, Havelock Island (Lagoon Villa)" },
                    new() { DayNumber = 3, Title = "Elephant Beach Coral Reef & Snorkeling", Description = "Speedboat ride to Elephant Beach. Complimentary snorkeling session among colourful coral reefs and tropical fish. Optional sea-walk or scuba diving.", Meals = "Breakfast", ActivityHighlight = "Complimentary Coral Snorkeling", Accommodation = "Symphony Palms Beach Resort, Havelock Island (Lagoon Villa)" },
                    new() { DayNumber = 4, Title = "Ferry to Port Blair & Chidiya Tapu Sunset", Description = "Ferry cruise back to Port Blair. Afternoon excursion to Chidiya Tapu (Bird Island) for a panoramic sunset over the Andaman Sea and mangrove forests.", Meals = "Breakfast", ActivityHighlight = "Chidiya Tapu Sunset Point", Accommodation = "SeaShell Port Blair (4-Star Bay View Resort)" },
                    new() { DayNumber = 5, Title = "Corbyn's Cove Beach & Departure", Description = "Morning drive along coconut-lined Corbyn's Cove Beach. Souvenir shopping for seashell crafts and transfer to Port Blair airport.", Meals = "Breakfast", ActivityHighlight = "Corbyn's Cove Coastal Drive", Accommodation = "Check-out & Veer Savarkar International Airport Departure" }
                },
                Inclusions = new() { "2 Nights Port Blair Hotel & 2 Nights Havelock Beachfront Resort", "Daily Buffet Breakfast", "Premium Catamaran Tickets (Port Blair - Havelock - Port Blair)", "Cellular Jail Entry & Light & Sound Show Tickets", "Speedboat to Elephant Beach with Snorkeling Gear" },
                Exclusions = new() { "Scuba diving and undersea walk charges", "Airfare to/from Port Blair", "Lunches and dinners not listed", "Personal expenses" }
            },

            
            new()
            {
                Id = 8,
                Name = "Rishikesh Yoga, Rafting & Riverside Camping",
                Destination = "Rishikesh",
                Duration = "3 Days / 2 Nights",
                DurationDays = 3,
                Price = 16999,
                ImageUrl = "https://images.unsplash.com/photo-1544735716-392fe2489ffa?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "16 km Ganga river rafting thrill, riverside luxury camping, cliff jumping, and Triveni Ghat Aarti.",
                Overview = "Recharge your spirit in the Yoga Capital of the World. Combine thrilling white-water rapids, riverside campfires under starry skies, and deeply peaceful Ganga Aarti ceremonies.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Riverside Camp Check-in & Triveni Ghat", Description = "Check in to luxury riverside camps in Shivpuri. Visit suspension bridges Ram Jhula and Laxman Jhula. Attend the mesmerizing evening Maha Aarti at Triveni Ghat.", Meals = "Dinner", ActivityHighlight = "Triveni Ghat Evening Aarti", Accommodation = "Camp Tattva Luxury Riverside Tents, Shivpuri" },
                    new() { DayNumber = 2, Title = "16 km White-Water Rafting & Cliff Jump", Description = "Exhilarating 16 km rafting from Shivpuri down to Rishikesh, navigating rapids like Roller Coaster and Golf Course. Cliff jump and body surf in pristine waters. Night bonfire with live music.", Meals = "Breakfast, Lunch & Dinner", ActivityHighlight = "16 km Grade III/IV River Rafting", Accommodation = "Camp Tattva Luxury Riverside Tents, Shivpuri" },
                    new() { DayNumber = 3, Title = "Sunrise Yoga & Beatles Ashram Tour", Description = "Sunrise yoga and meditation session by the river with a certified yogi. Explore the graffiti-filled Beatles Ashram (Chaurasi Kutia) before departure.", Meals = "Breakfast", ActivityHighlight = "Sunrise Yoga & Beatles Ashram", Accommodation = "Check-out & Dehradun Jolly Grant Airport / Haridwar Departure" }
                },
                Inclusions = new() { "2 Nights in Luxury Riverside Camps with attached washrooms", "All Meals: 2 Breakfasts, 1 Lunch, 2 Dinners with Barbecue", "16 km River Rafting with Licensed River Guide & Safety Gear", "Evening Campfire with snacks and acoustic music", "Guided Morning Yoga and Pranayama Session" },
                Exclusions = new() { "Bungee jumping and Flying Fox passes at Jumpin Heights", "Transport from your home city to Rishikesh", "Personal expenses" }
            },

            
            new()
            {
                Id = 9,
                Name = "Spiritual Varanasi Ghats & Sarnath Heritage",
                Destination = "Varanasi",
                Duration = "3 Days / 2 Nights",
                DurationDays = 3,
                Price = 18500,
                ImageUrl = "https://images.unsplash.com/photo-1561359313-0639aad49ca6?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Dawn boat ride on sacred Ganges, VIP seating at Dashashwamedh Aarti, and Lord Buddha's Sarnath.",
                Overview = "Walk through the world's oldest living city. Experience timeless rituals along sacred river ghats, ancient brass-lamp prayers, and the peaceful deer park where Buddhism was founded.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Arrival in Kashi & Grand Dashashwamedh Aarti", Description = "Arrive in Varanasi and transfer to hotel near the ghats. Evening private boat to witness the majestic brass-lamp Maha Aarti at Dashashwamedh Ghat.", Meals = "Dinner", ActivityHighlight = "Dashashwamedh Ghat Evening Aarti", Accommodation = "BrijRama Palace Heritage Hotel by the Ganges (Darbhanga Palace)" },
                    new() { DayNumber = 2, Title = "Subah-e-Banaras Boat Ride & Sarnath", Description = "Sunrise boat tour witnessing spiritual rituals along Manikarnika and Assi ghats. Afternoon excursion to Sarnath to see Dhamek Stupa and Ashoka Pillar.", Meals = "Breakfast & Dinner", ActivityHighlight = "Sunrise Ganges Boat Cruise", Accommodation = "BrijRama Palace Heritage Hotel by the Ganges (Darbhanga Palace)" },
                    new() { DayNumber = 3, Title = "Kashi Vishwanath Corridor & Departure", Description = "Morning darshan at the renovated golden Kashi Vishwanath Temple and Annapurna Temple. Taste authentic Banarasi lassi before airport drop.", Meals = "Breakfast", ActivityHighlight = "Kashi Vishwanath Temple Darshan", Accommodation = "Check-out & Lal Bahadur Shastri International Airport Departure" }
                },
                Inclusions = new() { "2 Nights in Heritage Boutique Hotel near Ghats", "Daily Breakfast & Dinner", "Two Guided Boat Cruises on Ganges (Sunrise & Aarti)", "Sarnath Guided Tour with Archaeological Museum Entry", "Private AC Cab for all city transfers" },
                Exclusions = new() { "Temple VIP priority passes", "Airfare / Train tickets to Varanasi", "Personal donations and shopping" }
            },

            
            new()
            {
                Id = 10,
                Name = "Darjeeling Queen of the Hills & Tea Trails",
                Destination = "Darjeeling",
                Duration = "4 Days / 3 Nights",
                DurationDays = 4,
                Price = 22500,
                ImageUrl = "https://images.unsplash.com/photo-1544634076-a90160ddf44c?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Tiger Hill sunrise over Mt. Kanchenjunga, UNESCO Toy Train ride, and Happy Valley tea tasting.",
                Overview = "Soak in the colonial elegance of the Queen of the Hills. Gaze at Kanchenjunga painted gold by the morning sun, ride the historic steam Toy Train, and sip world-famous Muscatel tea.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Bagdogra Arrival & Scenic Hill Drive", Description = "Pick up from Bagdogra Airport / NJP Station. Scenic winding drive through emerald tea slopes to Darjeeling. Check in and stroll Chowrasta Mall Road.", Meals = "Dinner", ActivityHighlight = "Chowrasta Mall Road Stroll", Accommodation = "The Elgin Darjeeling (Heritage Colonial Palace Hotel, 1887)" },
                    new() { DayNumber = 2, Title = "Tiger Hill Sunrise & Himalayan Zoo", Description = "Early 4:00 AM trip to Tiger Hill for sunrise over Kanchenjunga. Visit Ghoom Monastery and Batasia Loop. Afternoon tour of Himalayan Mountaineering Institute and Red Panda Zoo.", Meals = "Breakfast & Dinner", ActivityHighlight = "Tiger Hill Kanchenjunga Sunrise", Accommodation = "The Elgin Darjeeling (Heritage Colonial Palace Hotel, 1887)" },
                    new() { DayNumber = 3, Title = "UNESCO Toy Train Joyride & Tea Estate", Description = "Ride the historic steam DHR Toy Train to Ghoom. Walk through Happy Valley Tea Estate, witness tea manufacturing, and sample freshly brewed first flush teas.", Meals = "Breakfast & Dinner", ActivityHighlight = "UNESCO Steam Toy Train Ride", Accommodation = "The Elgin Darjeeling (Heritage Colonial Palace Hotel, 1887)" },
                    new() { DayNumber = 4, Title = "Peace Pagoda & Bagdogra Drop", Description = "Visit the serene Japanese Peace Pagoda. Soak in final Himalayan views before transfer to Bagdogra Airport for departure.", Meals = "Breakfast", ActivityHighlight = "Japanese Peace Pagoda", Accommodation = "Check-out & Bagdogra International Airport Departure" }
                },
                Inclusions = new() { "3 Nights in Colonial Heritage Hill Hotel", "Daily Breakfast & Dinner", "Tiger Hill Sunrise Tour by Private Vehicle", "UNESCO Heritage Toy Train Joyride Tickets", "Private Vehicle for Bagdogra/NJP transfers" },
                Exclusions = new() { "Airfare / Train tickets to Bagdogra", "Private tea-tasting connoisseur fees", "Personal laundry and tips" }
            },

            
            new()
            {
                Id = 11,
                Name = "Bali Tropical Serenity & Ubud Cultural Trail",
                Destination = "Bali",
                Duration = "6 Days / 5 Nights",
                DurationDays = 6,
                Price = 48500,
                ImageUrl = "/Images/bali.jpg",
                ShortDescription = "Seminyak sunsets, Uluwatu fire dance, Ubud jungle swing, and Nusa Penida island hopping.",
                Overview = "Experience the best of Bali. Balance vibrant beach vibes in Seminyak with spiritual tranquility in Ubud, towering sea cliffs, and the dramatic T-Rex cliff at Nusa Penida.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Denpasar Arrival & Seminyak Beach", Description = "Land at Ngurah Rai Airport. Private transfer to beachfront resort in Seminyak. Unwind on Double Six Beach and enjoy sunset cocktails.", Meals = "Welcome Drink", ActivityHighlight = "Seminyak Beach Sunset", Accommodation = "W Bali - Seminyak (5-Star Beachfront Luxury Resort)" },
                    new() { DayNumber = 2, Title = "Uluwatu Cliff Temple & Kecak Dance", Description = "Leisure morning. Afternoon tour of dramatic cliffside Uluwatu Temple perched 70m above crashing waves. Watch the hypnotic Kecak Fire Dance at sunset.", Meals = "Breakfast", ActivityHighlight = "Uluwatu Sunset Kecak Fire Dance", Accommodation = "W Bali - Seminyak (5-Star Beachfront Luxury Resort)" },
                    new() { DayNumber = 3, Title = "Ubud Rice Terraces & Jungle Bali Swing", Description = "Scenic drive to Ubud. Stroll the emerald Tegalalang Rice Terraces, fly high on the famous jungle Bali Swing, and visit Sacred Monkey Forest.", Meals = "Breakfast", ActivityHighlight = "Tegalalang Rice Terraces & Swing", Accommodation = "Komaneka at Bisma Ubud (Private Valley Pool Villa)" },
                    new() { DayNumber = 4, Title = "Nusa Penida Island Tour", Description = "Fast boat cruise to Nusa Penida. Photograph the iconic T-Rex shaped Kelingking Beach cliff, swim in natural infinity pool Angel's Billabong, and visit Broken Beach.", Meals = "Breakfast & Island Lunch", ActivityHighlight = "Kelingking T-Rex Cliff", Accommodation = "Komaneka at Bisma Ubud (Private Valley Pool Villa)" },
                    new() { DayNumber = 5, Title = "Tanah Lot Temple & Balinese Spa", Description = "Visit the legendary offshore rock temple Tanah Lot. Indulge in an authentic 2-hour Balinese herbal aromatherapy massage and Jimbaran Bay seafood dinner.", Meals = "Breakfast & Seafood Dinner", ActivityHighlight = "2-Hour Authentic Balinese Spa", Accommodation = "Komaneka at Bisma Ubud (Private Valley Pool Villa)" },
                    new() { DayNumber = 6, Title = "Kuta Art Market & Departure", Description = "Browse Kuta Art Market for handmade souvenirs and silver jewelry. Transfer to airport for flight home.", Meals = "Breakfast", ActivityHighlight = "Handicraft Souvenir Shopping", Accommodation = "Check-out & Denpasar Ngurah Rai Airport Departure" }
                },
                Inclusions = new() { "3 Nights Seminyak Hotel & 2 Nights Ubud Private Pool Villa", "Daily International Buffet Breakfast", "Full-Day Nusa Penida Island Speedboat Tour with Lunch", "Uluwatu Temple & Kecak Fire Dance VIP Passes", "2-Hour Balinese Aromatherapy Spa Treatment", "Private Airport and Sightseeing Transfers" },
                Exclusions = new() { "International flight tickets", "Indonesia Visa on Arrival ($35 USD)", "Personal water sports rentals", "Travel insurance" }
            },

            
            new()
            {
                Id = 12,
                Name = "Dazzling Dubai & Arabian Desert Safari",
                Destination = "Dubai",
                Duration = "5 Days / 4 Nights",
                DurationDays = 5,
                Price = 58000,
                ImageUrl = "/Images/dubai.jpg",
                ShortDescription = "Burj Khalifa 124th floor, red dunes 4x4 desert safari, Dubai Marina dinner cruise, and Gold Souk.",
                Overview = "Experience futuristic luxury in the City of Gold. Ascend the world's tallest tower, race across Arabian red dunes, cruise under gleaming skyscrapers, and explore traditional souks.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Dubai Arrival & Marina Dhow Cruise", Description = "Arrive at Dubai International Airport. Check into 4-star city hotel. Evening luxury glass-enclosed Dhow dinner cruise with skyline views of Dubai Marina.", Meals = "Dinner on Cruise", ActivityHighlight = "Dubai Marina Dhow Dinner Cruise", Accommodation = "Sofitel Dubai Downtown (5-Star Luxury with Burj Khalifa View)" },
                    new() { DayNumber = 2, Title = "City Tour & Burj Khalifa 124th Floor", Description = "Half-day city tour covering Dubai Frame, Palm Jumeirah, and Atlantis. Afternoon fast-track entry to Burj Khalifa At the Top (124th/125th floor) and Dubai Fountain show.", Meals = "Breakfast", ActivityHighlight = "Burj Khalifa At The Top 124F", Accommodation = "Sofitel Dubai Downtown (5-Star Luxury with Burj Khalifa View)" },
                    new() { DayNumber = 3, Title = "Red Dunes Desert Safari & BBQ Dinner", Description = "Morning at leisure. Afternoon 4x4 Land Cruiser safari into Lahbab red dunes: dune bashing, sandboarding, camel rides, and open-air BBQ dinner with Tanoura dance.", Meals = "Breakfast & BBQ Dinner", ActivityHighlight = "4x4 Red Dunes Safari & BBQ", Accommodation = "Sofitel Dubai Downtown (5-Star Luxury with Burj Khalifa View)" },
                    new() { DayNumber = 4, Title = "Miracle Garden & Global Village", Description = "Tour the world's largest natural flower garden at Dubai Miracle Garden. Evening exploration of Global Village's cultural pavilions and street food.", Meals = "Breakfast", ActivityHighlight = "Dubai Miracle Garden", Accommodation = "Sofitel Dubai Downtown (5-Star Luxury with Burj Khalifa View)" },
                    new() { DayNumber = 5, Title = "Gold Souk, Creek Abra & Departure", Description = "Ride traditional Abra wooden boat across Dubai Creek. Shop for jewelry and spices in historic Gold and Spice Souks before transfer to airport.", Meals = "Breakfast", ActivityHighlight = "Abra Ride & Gold Souk", Accommodation = "Check-out & Dubai International Airport (DXB) Departure" }
                },
                Inclusions = new() { "4 Nights in 4-Star Downtown/Marina Hotel", "Daily Buffet Breakfast & 2 Dinners (Dhow & Safari)", "Burj Khalifa At The Top 124th Floor Admission Ticket", "Premium Desert Safari with 4x4 Dune Bashing & BBQ", "Dubai Marina Luxury Dhow Cruise with Dinner", "Private Airport and Sightseeing Transfers" },
                Exclusions = new() { "UAE Tourist Visa fee", "Tourism Dirham tax paid directly at hotel ($4-5/night)", "International flight tickets", "Personal shopping" }
            },

            
            new()
            {
                Id = 13,
                Name = "Swiss Alpine Wonders & Glacier Explorer",
                Destination = "Switzerland",
                Duration = "7 Days / 6 Nights",
                DurationDays = 7,
                Price = 145000,
                ImageUrl = "/Images/switzerland.jpg",
                ShortDescription = "Jungfraujoch Top of Europe, Mt. Titlis revolving cable car, Lake Lucerne, and Zermatt Matterhorn.",
                Overview = "Discover the crown jewel of the Alps. Journey on world-famous panoramic trains across snow-capped peaks, eternal ice caves, sparkling turquoise lakes, and fairy-tale wooden chalets.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Zurich Arrival & Scenic Train to Lucerne", Description = "Land in Zurich. Board scenic Swiss train to picturesque Lucerne. Walk across the 14th-century wooden Chapel Bridge and along Lake Lucerne promenade.", Meals = "Welcome Drink", ActivityHighlight = "Lucerne Chapel Bridge", Accommodation = "Hotel Montana Lucerne (Art Deco Panorama Suite on Lake Lucerne)" },
                    new() { DayNumber = 2, Title = "Mount Titlis Rotair Cableway & Ice Cave", Description = "Ascend Mt. Titlis (10,000 ft) on the world's first revolving cable car. Walk Europe's highest suspension bridge (Titlis Cliff Walk) and explore the glacier ice cave.", Meals = "Breakfast", ActivityHighlight = "Mt. Titlis Revolving Cable Car", Accommodation = "Hotel Montana Lucerne (Art Deco Panorama Suite on Lake Lucerne)" },
                    new() { DayNumber = 3, Title = "Lucerne to Interlaken & Lake Brienz", Description = "Scenic train through alpine pastures to Interlaken between Lakes Thun and Brienz. Cruise the turquoise waters of Lake Brienz by historic paddle steamer.", Meals = "Breakfast", ActivityHighlight = "Lake Brienz Steamer Cruise", Accommodation = "Victoria-Jungfrau Grand Hotel & Spa, Interlaken" },
                    new() { DayNumber = 4, Title = "Jungfraujoch - Top of Europe Expedition", Description = "Board the high-speed Eiger Express gondola and cogwheel train to Jungfraujoch (3,454m). Visit the Ice Palace, Sphinx Observatory, and eternal snow plateaus.", Meals = "Breakfast", ActivityHighlight = "Jungfraujoch Top of Europe", Accommodation = "Victoria-Jungfrau Grand Hotel & Spa, Interlaken" },
                    new() { DayNumber = 5, Title = "Interlaken to Zermatt & Matterhorn View", Description = "Train journey to car-free Zermatt. Ride the Gornergrat cogwheel railway for stunning close-up views of the iconic pyramid-shaped Matterhorn.", Meals = "Breakfast", ActivityHighlight = "Gornergrat Matterhorn Vista", Accommodation = "The Omnia Mountain Lodge, Zermatt (Matterhorn View Suite)" },
                    new() { DayNumber = 6, Title = "Alpine Wildflower Hike & Swiss Fondue", Description = "Gentle alpine walk along the Five Lakes trail. Evening authentic Swiss cheese fondue dinner in a cozy mountain chalet.", Meals = "Breakfast & Fondue Dinner", ActivityHighlight = "Traditional Swiss Fondue Night", Accommodation = "The Omnia Mountain Lodge, Zermatt (Matterhorn View Suite)" },
                    new() { DayNumber = 7, Title = "Zurich / Geneva Airport Departure", Description = "Scenic train back to Zurich or Geneva airport. Duty-free shopping for Swiss chocolates and watches before flight home.", Meals = "Breakfast", ActivityHighlight = "Swiss Chocolate Shopping", Accommodation = "Check-out & Zurich / Geneva Airport Departure" }
                },
                Inclusions = new() { "6 Nights in 4-Star Swiss Alpine Hotels (Lucerne, Interlaken, Zermatt)", "Daily Swiss Buffet Breakfast", "Consecutive Swiss Travel Pass (1st Class) with Unlimited Trains & Boats", "Jungfraujoch Top of Europe Mountain Excursion Tickets", "Mount Titlis Cable Car Pass with Ice Flyer", "Traditional Swiss Fondue Dinner" },
                Exclusions = new() { "Schengen Visa and travel medical insurance", "International airfare to/from Switzerland", "Meals not listed in inclusions", "City tourist taxes" }
            },

            
            new()
            {
                Id = 14,
                Name = "Santorini Romantic Caldera & Aegean Cruise",
                Destination = "Santorini",
                Duration = "5 Days / 4 Nights",
                DurationDays = 5,
                Price = 115000,
                ImageUrl = "/Images/santorini.jpg",
                ShortDescription = "Cliffside caldera suite, luxury catamaran cruise with Greek BBQ, Oia sunset, and volcanic wineries.",
                Overview = "Experience the ultimate Mediterranean dream. Gaze at whitewashed villas and blue-domed churches clinging to volcanic cliffs, swim in thermal springs, and witness Oia's legendary sunset.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Santorini Arrival & Fira Cliffside Stroll", Description = "Land at Thira Airport or port. Private transfer to your whitewashed cliffside hotel. Relax on your private balcony overlooking the caldera and walk Fira's cobbled lanes.", Meals = "Welcome Wine", ActivityHighlight = "Caldera Cliffside View", Accommodation = "Katikies Chromata Santorini (5-Star Caldera Cave Suite, Imerovigli)" },
                    new() { DayNumber = 2, Title = "Luxury Catamaran Caldera Cruise & Springs", Description = "Sail on a luxury catamaran across the submerged volcanic caldera. Swim in volcanic hot springs, snorkel at Red Beach, and savor an on-board Greek BBQ with Aegean wines.", Meals = "Breakfast & On-board Greek BBQ", ActivityHighlight = "Catamaran Cruise & Hot Springs", Accommodation = "Katikies Chromata Santorini (5-Star Caldera Cave Suite, Imerovigli)" },
                    new() { DayNumber = 3, Title = "Picturesque Oia Village & Famous Sunset", Description = "Full day in iconic Oia. Photograph blue-domed churches, browse boutique art galleries, and enjoy reserved sunset seating for the world's most famous sunset.", Meals = "Breakfast", ActivityHighlight = "World-Famous Oia Sunset", Accommodation = "Katikies Chromata Santorini (5-Star Caldera Cave Suite, Imerovigli)" },
                    new() { DayNumber = 4, Title = "Akrotiri Ruins & Santo Wines Tasting", Description = "Tour prehistoric Minoan ruins preserved in volcanic ash at Akrotiri. Afternoon sommelier-guided wine tasting at Santo Wines high above the Aegean Sea.", Meals = "Breakfast", ActivityHighlight = "Santo Wines Cliffside Tasting", Accommodation = "Katikies Chromata Santorini (5-Star Caldera Cave Suite, Imerovigli)" },
                    new() { DayNumber = 5, Title = "Black Sand Beach & Departure", Description = "Morning swim at volcanic Kamari Black Sand Beach. Farewell Greek frappe coffee and private transfer to Santorini airport.", Meals = "Breakfast", ActivityHighlight = "Kamari Black Sand Beach", Accommodation = "Check-out & Santorini Thira Airport Departure" }
                },
                Inclusions = new() { "4 Nights in Premium Caldera View Suite with Private Balcony", "Daily Champagne Breakfast served on your terrace", "Semi-Private 5-Hour Catamaran Cruise with Greek BBQ & Wine", "Santorini Wine Tasting Tour with Certified Sommelier", "Private Airport / Ferry Port Transfers" },
                Exclusions = new() { "Greece Stayover City Tax (payable at hotel)", "International airfare to Greece", "Personal expenses and lunches on free days" }
            },

            
            new()
            {
                Id = 15,
                Name = "Parisian Romance, Art & Seine River Cruise",
                Destination = "Paris",
                Duration = "6 Days / 5 Nights",
                DurationDays = 6,
                Price = 120000,
                ImageUrl = "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Eiffel Tower summit priority access, Louvre museum masterpieces, Versailles palace, and Seine cruise.",
                Overview = "Fall in love with the City of Light. Ascend the Eiffel Tower summit, gaze at the Mona Lisa in the Louvre, cruise along the illuminated Seine, and explore bohemian Montmartre.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Paris Arrival & Champs-Élysées Evening", Description = "Arrive at Paris CDG Airport. Private transfer to Parisian boutique hotel. Evening walk down Avenue des Champs-Élysées to the illuminated Arc de Triomphe.", Meals = "Welcome Drink", ActivityHighlight = "Champs-Élysées & Arc de Triomphe", Accommodation = "Le Cinq Codet Paris 7th Arrondissement (5-Star Luxury Eiffel View Hotel)" },
                    new() { DayNumber = 2, Title = "Eiffel Tower Summit & Seine Cruise", Description = "Priority elevator access to the Eiffel Tower summit (3rd floor) for 360-degree Paris views. Evening glass-topped Bateaux Parisiens cruise down the River Seine.", Meals = "Breakfast", ActivityHighlight = "Eiffel Tower Summit Priority", Accommodation = "Le Cinq Codet Paris 7th Arrondissement (5-Star Luxury Eiffel View Hotel)" },
                    new() { DayNumber = 3, Title = "Louvre Museum Masterpieces & Montmartre", Description = "Skip-the-line guided access to the Louvre seeing Mona Lisa, Venus de Milo, and Winged Victory. Afternoon stroll through Montmartre and Sacré-Cœur Basilica.", Meals = "Breakfast", ActivityHighlight = "Louvre Museum Guided Tour", Accommodation = "Le Cinq Codet Paris 7th Arrondissement (5-Star Luxury Eiffel View Hotel)" },
                    new() { DayNumber = 4, Title = "Palace of Versailles Royal Day Excursion", Description = "Day trip to the opulent Palace of Versailles. Tour the magnificent Hall of Mirrors, King's State Apartments, and landscaped royal fountains.", Meals = "Breakfast", ActivityHighlight = "Versailles Hall of Mirrors", Accommodation = "Le Cinq Codet Paris 7th Arrondissement (5-Star Luxury Eiffel View Hotel)" },
                    new() { DayNumber = 5, Title = "Latin Quarter & French Bakery Masterclass", Description = "Explore the Latin Quarter and trendy Le Marais. Participate in a hands-on French croissant and macaron baking masterclass with a master baker.", Meals = "Breakfast & Baked Delights", ActivityHighlight = "French Croissant Baking Class", Accommodation = "Le Cinq Codet Paris 7th Arrondissement (5-Star Luxury Eiffel View Hotel)" },
                    new() { DayNumber = 6, Title = "Galeries Lafayette & Flight Home", Description = "Morning shopping under the famous Art Nouveau glass dome of Galeries Lafayette. Private transfer to CDG Airport for return flight.", Meals = "Breakfast", ActivityHighlight = "Galeries Lafayette Shopping", Accommodation = "Check-out & Paris CDG Airport Departure" }
                },
                Inclusions = new() { "5 Nights in Central 4-Star Paris Hotel", "Daily French Continental Breakfast", "Eiffel Tower Summit Priority Access Tickets", "Louvre Museum Timed-Entry Ticket", "Seine River 1-Hour Cruise with Audio Guide", "Palace of Versailles Passport Entry Ticket", "Private Airport Transfers" },
                Exclusions = new() { "Schengen Visa fees", "International flights to/from Paris", "Paris City Tourist Tax (paid at hotel)", "Meals not specified" }
            },

            
            new()
            {
                Id = 16,
                Name = "Tokyo Highlights & Mt. Fuji Wonders",
                Destination = "Tokyo",
                Duration = "6 Days / 5 Nights",
                DurationDays = 6,
                Price = 135000,
                ImageUrl = "https://images.unsplash.com/photo-1503899036084-c55cdd92da26?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Shibuya Crossing, teamLab digital art, Mount Fuji 5th station, Hakone cruise, and Asakusa shrines.",
                Overview = "Experience Japan's incredible harmony of ancient tradition and futuristic marvels. Cross the world's busiest intersection, immerse in digital art wonderland, and marvel at snow-capped Mount Fuji.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Tokyo Arrival & Shinjuku Neon Night", Description = "Land at Haneda or Narita Airport. Take high-speed Narita Express to central Tokyo hotel. Evening walk through neon-lit Shinjuku and Omoide Yokocho lantern alleyways.", Meals = "Welcome Drink", ActivityHighlight = "Shinjuku Neon Alleyways", Accommodation = "Andaz Tokyo Toranomon Hills - A Concept by Hyatt (5-Star Luxury Tower)" },
                    new() { DayNumber = 2, Title = "Asakusa Senso-ji & Shibuya Crossing", Description = "Visit Tokyo's oldest temple, Senso-ji, and browse Nakamise shopping arcade. Cross the famous Shibuya Scramble and take in Shibuya Sky panoramic deck.", Meals = "Breakfast", ActivityHighlight = "Shibuya Crossing & Sky Deck", Accommodation = "Andaz Tokyo Toranomon Hills - A Concept by Hyatt (5-Star Luxury Tower)" },
                    new() { DayNumber = 3, Title = "Mount Fuji 5th Station & Lake Ashi Cruise", Description = "Full-day coach excursion to majestic Mount Fuji 5th Station. Cruise across Lake Ashi on a pirate ship and take the Hakone Ropeway over volcanic vents.", Meals = "Breakfast & Bento Lunch", ActivityHighlight = "Mount Fuji & Lake Ashi Cruise", Accommodation = "Andaz Tokyo Toranomon Hills - A Concept by Hyatt (5-Star Luxury Tower)" },
                    new() { DayNumber = 4, Title = "teamLab Borderless Digital Art & Ginza", Description = "Immerse in the world-renowned teamLab Borderless interactive digital art museum in Azabudai Hills. Afternoon luxury shopping in Ginza district.", Meals = "Breakfast", ActivityHighlight = "teamLab Borderless Digital Art", Accommodation = "Andaz Tokyo Toranomon Hills - A Concept by Hyatt (5-Star Luxury Tower)" },
                    new() { DayNumber = 5, Title = "Meiji Jingu Shrine & Akihabara Tech Town", Description = "Walk through tranquil cedar forests to sacred Meiji Jingu Shrine. Afternoon exploration of anime and gaming capital Akihabara. Farewell authentic ramen dinner.", Meals = "Breakfast & Ramen Dinner", ActivityHighlight = "Meiji Jingu & Akihabara", Accommodation = "Andaz Tokyo Toranomon Hills - A Concept by Hyatt (5-Star Luxury Tower)" },
                    new() { DayNumber = 6, Title = "Tsukiji Outer Market & Tokyo Departure", Description = "Morning fresh sushi tasting at Tsukiji Outer Market. Private transfer to airport for flight home.", Meals = "Breakfast", ActivityHighlight = "Tsukiji Market Fresh Sushi", Accommodation = "Check-out & Tokyo Haneda / Narita Airport Departure" }
                },
                Inclusions = new() { "5 Nights in 4-Star Central Tokyo Hotel", "Daily Japanese/Western Buffet Breakfast", "Full-Day Mount Fuji & Hakone Tour with Lunch", "teamLab Borderless Digital Art Admission Ticket", "72-Hour Tokyo Subway Unlimited Pass", "Airport Train Express Transfer" },
                Exclusions = new() { "Japan Tourist Visa", "International flight tickets", "Personal shopping and dining not specified" }
            },

            
            new()
            {
                Id = 17,
                Name = "Spectacular Singapore & Sentosa Island Tour",
                Destination = "Singapore",
                Duration = "5 Days / 4 Nights",
                DurationDays = 5,
                Price = 52000,
                ImageUrl = "https://images.unsplash.com/photo-1525625293386-3f8f99389edd?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Gardens by the Bay Supertrees, Universal Studios Sentosa, Marina Bay Sands, and Night Safari.",
                Overview = "Step into a futuristic garden metropolis. Marvel at indoor waterfalls in Cloud Forest, ride blockbuster rides at Universal Studios Sentosa, and view the Singapore skyline from Marina Bay Sands.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Singapore Arrival & Night Safari", Description = "Land at Changi Jewel Airport. Transfer to 4-star city hotel. Evening tram safari through the world's premier nocturnal wildlife park (Singapore Night Safari).", Meals = "Welcome Drink", ActivityHighlight = "Singapore Night Safari Tram", Accommodation = "Marina Bay Sands Hotel (Premier Room with SkyPark & Infinity Pool Access)" },
                    new() { DayNumber = 2, Title = "Gardens by the Bay & Marina Bay Sands", Description = "Visit Flower Dome and Cloud Forest with its 35m indoor waterfall. Stroll under the Supertree Grove and watch the Garden Rhapsody light & sound show.", Meals = "Breakfast", ActivityHighlight = "Supertree Garden Rhapsody Show", Accommodation = "Marina Bay Sands Hotel (Premier Room with SkyPark & Infinity Pool Access)" },
                    new() { DayNumber = 3, Title = "Universal Studios Sentosa Island", Description = "Scenic cable car over the harbor to Sentosa Island. Full day of thrilling movie-themed rides at Universal Studios Singapore and Wings of Time laser show.", Meals = "Breakfast", ActivityHighlight = "Universal Studios Singapore 1-Day", Accommodation = "Marina Bay Sands Hotel (Premier Room with SkyPark & Infinity Pool Access)" },
                    new() { DayNumber = 4, Title = "Chinatown, Little India & River Cruise", Description = "Explore the colourful heritage of Chinatown and Little India. Evening Bumboat cruise along the Singapore River passing Clarke Quay and Merlion Park.", Meals = "Breakfast", ActivityHighlight = "Singapore River Bumboat Cruise", Accommodation = "Marina Bay Sands Hotel (Premier Room with SkyPark & Infinity Pool Access)" },
                    new() { DayNumber = 5, Title = "Jewel Changi Rain Vortex & Departure", Description = "Check in early to explore the 40-meter indoor HSBC Rain Vortex waterfall and Canopy Park at Jewel Changi before departure.", Meals = "Breakfast", ActivityHighlight = "Jewel Changi 40m Rain Vortex", Accommodation = "Check-out & Jewel Changi International Airport Departure" }
                },
                Inclusions = new() { "4 Nights in 4-Star Singapore City Hotel", "Daily Buffet Breakfast", "Gardens by the Bay Double Conservatories (Cloud Forest & Flower Dome)", "Universal Studios Singapore 1-Day Pass", "Night Safari Admission with Tram Ride", "Private Airport and Sightseeing Transfers" },
                Exclusions = new() { "Singapore Entry Visa", "International flight tickets", "Meals not listed in inclusions", "Personal shopping" }
            },

            
            new()
            {
                Id = 18,
                Name = "Thailand Tropical Escape: Bangkok & Phuket",
                Destination = "Thailand",
                Duration = "6 Days / 5 Nights",
                DurationDays = 6,
                Price = 38000,
                ImageUrl = "https://images.unsplash.com/photo-1506665531195-3566af2b4dfa?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Bangkok Grand Palace, Chaophraya dinner cruise, Phuket beaches, and Phi Phi island speedboat tour.",
                Overview = "Experience the best of Thailand's royal splendor and tropical island paradise. Explore gilded temples in Bangkok, feast on world-famous street food, and cruise emerald waters to Maya Bay.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Bangkok Arrival & Chaophraya River Cruise", Description = "Land at Suvarnabhumi Airport. Transfer to 4-star hotel. Evening luxury Chaophraya Princess river dinner cruise with live music and traditional Thai dance.", Meals = "Dinner on Cruise", ActivityHighlight = "Chaophraya Princess Dinner Cruise", Accommodation = "The Peninsula Bangkok (5-Star Luxury Riverfront Suite)" },
                    new() { DayNumber = 2, Title = "Grand Palace & Wat Arun Temple of Dawn", Description = "Guided tour of the glittering Grand Palace and Emerald Buddha temple. Take cross-river ferry to porcelain-encrusted Wat Arun and visit Wat Pho Reclining Buddha.", Meals = "Breakfast", ActivityHighlight = "Grand Palace & Reclining Buddha", Accommodation = "The Peninsula Bangkok (5-Star Luxury Riverfront Suite)" },
                    new() { DayNumber = 3, Title = "Flight to Phuket & Patong Beach Vibe", Description = "Short domestic flight to tropical Phuket. Check into beach resort. Enjoy sunset cocktails and explore night markets along Patong's lively Bangla Road.", Meals = "Breakfast", ActivityHighlight = "Patong Beach Sunset", Accommodation = "Trisara Resort & Villas, Phuket (Private Ocean View Pool Villa)" },
                    new() { DayNumber = 4, Title = "Phi Phi Islands & Maya Bay Speedboat", Description = "Speedboat expedition to Phi Phi Don and Phi Phi Leh. Swim at legendary Maya Bay, snorkel vibrant coral reefs, and see Viking Cave with buffet lunch.", Meals = "Breakfast & Beach Buffet Lunch", ActivityHighlight = "Phi Phi Islands & Maya Bay", Accommodation = "Trisara Resort & Villas, Phuket (Private Ocean View Pool Villa)" },
                    new() { DayNumber = 5, Title = "Phuket Big Buddha & Old Town Heritage", Description = "Visit the hilltop 45-meter marble Big Buddha with panoramic ocean vistas. Stroll colourful Sino-Portuguese shophouses and cafes in Old Phuket Town.", Meals = "Breakfast", ActivityHighlight = "Hilltop Marble Big Buddha", Accommodation = "Trisara Resort & Villas, Phuket (Private Ocean View Pool Villa)" },
                    new() { DayNumber = 6, Title = "Farewell Phuket & Flight Home", Description = "Morning swim at Kata Beach and tropical fruit breakfast. Private transfer to Phuket International Airport for flight home.", Meals = "Breakfast", ActivityHighlight = "Tropical Beach Morning", Accommodation = "Check-out & Phuket International Airport Departure" }
                },
                Inclusions = new() { "2 Nights Bangkok & 3 Nights Phuket 4-Star Beach Resorts", "Daily International Buffet Breakfast", "Phi Phi Islands Speedboat Tour with National Park Fee & Lunch", "Chaophraya Princess River Dinner Cruise Pass", "All Airport and Inter-city Transfers in Private AC Cab" },
                Exclusions = new() { "International flights and Bangkok-Phuket domestic flight", "Thailand Visa on Arrival (if applicable)", "Personal watersports and water scooter rentals" }
            },

            
            new()
            {
                Id = 19,
                Name = "Rome Antiquities, Vatican & Tuscan Taste",
                Destination = "Rome",
                Duration = "5 Days / 4 Nights",
                DurationDays = 5,
                Price = 110000,
                ImageUrl = "https://images.unsplash.com/photo-1552832230-c0197dd311b5?auto=format&fit=crop&w=1000&q=80",
                ShortDescription = "Colosseum arena floor skip-the-line, Vatican Sistine Chapel, Trevi Fountain, and pasta making.",
                Overview = "Walk through millennia of history in the Eternal City. Step onto the Colosseum arena floor, gaze at Michelangelo's Sistine Chapel ceiling, toss a coin in Trevi Fountain, and master fresh pasta making.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "Rome Arrival & Piazza Navona Evening", Description = "Arrive at Rome Fiumicino Airport. Private transfer to historic center hotel. Evening walk past Piazza Navona's Bernini fountains and ancient Pantheon temple.", Meals = "Welcome Drink", ActivityHighlight = "Piazza Navona & Pantheon", Accommodation = "Hotel de Russie, Rocco Forte (5-Star Luxury Hotel near Piazza del Popolo)" },
                    new() { DayNumber = 2, Title = "Colosseum Arena Floor & Roman Forum", Description = "Skip-the-line VIP access to the Colosseum arena floor where gladiators fought. Guided tour of the Roman Forum and Roman emperors' palaces on Palatine Hill.", Meals = "Breakfast", ActivityHighlight = "Colosseum Arena Floor VIP", Accommodation = "Hotel de Russie, Rocco Forte (5-Star Luxury Hotel near Piazza del Popolo)" },
                    new() { DayNumber = 3, Title = "Vatican Museums, Sistine Chapel & St. Peter's", Description = "Morning guided tour of the Vatican Museums, Raphael Rooms, and Michelangelo's ceiling in Sistine Chapel. Enter St. Peter's Basilica without waiting.", Meals = "Breakfast", ActivityHighlight = "Vatican & Sistine Chapel", Accommodation = "Hotel de Russie, Rocco Forte (5-Star Luxury Hotel near Piazza del Popolo)" },
                    new() { DayNumber = 4, Title = "Trevi Fountain, Spanish Steps & Pasta Class", Description = "Toss a coin into Trevi Fountain to ensure your return to Rome. Climb Spanish Steps. Evening hands-on fresh pasta and tiramisu cooking class with local chef.", Meals = "Breakfast & Masterclass Dinner", ActivityHighlight = "Italian Pasta & Tiramisu Class", Accommodation = "Hotel de Russie, Rocco Forte (5-Star Luxury Hotel near Piazza del Popolo)" },
                    new() { DayNumber = 5, Title = "Villa Borghese Gardens & Departure", Description = "Stroll the lush gardens of Villa Borghese. Enjoy a final espresso and creamy gelato before transfer to Fiumicino Airport for departure.", Meals = "Breakfast", ActivityHighlight = "Villa Borghese & Italian Gelato", Accommodation = "Check-out & Rome Fiumicino Airport (FCO) Departure" }
                },
                Inclusions = new() { "4 Nights in 4-Star Historic Rome Hotel", "Daily Italian Buffet Breakfast", "Colosseum Arena Floor & Roman Forum Priority Entry Tickets with Guide", "Vatican Museums & Sistine Chapel Skip-The-Line Tour", "Traditional Roman Pasta & Tiramisu Cooking Masterclass with Dinner", "Private Airport Transfers" },
                Exclusions = new() { "Rome City Tourist Tax (payable directly to hotel)", "Schengen Visa fees", "International airfare to/from Rome" }
            },

            
            new()
            {
                Id = 20,
                Name = "El Nido Marine Lagoons & Island Hopping",
                Destination = "El Nido",
                Duration = "5 Days / 4 Nights",
                DurationDays = 5,
                Price = 65000,
                ImageUrl = "/Images/elnido.jpg",
                ShortDescription = "Kayak through Big Lagoon, swim through Secret Lagoon, and relax on pristine Nacpan Beach.",
                Overview = "Discover the world's most breathtaking tropical marine paradise in Palawan. Towering limestone monoliths, crystal turquoise lagoons, secluded white sand crescents, and vibrant coral gardens.",
                Itinerary = new List<ItineraryDay>
                {
                    new() { DayNumber = 1, Title = "El Nido Arrival & Nacpan Beach Sunset", Description = "Arrive at El Nido Lio Airport or Puerto Princesa. Scenic coastal transfer to beach resort. Relax on the 4 km golden strand of Nacpan Beach with sunset cocktails.", Meals = "Welcome Coconut", ActivityHighlight = "Nacpan Beach Golden Sunset", Accommodation = "Pangulasian Island Resort, El Nido (5-Star Eco-Luxury Beachfront Villa)" },
                    new() { DayNumber = 2, Title = "Island Hopping Tour A: Big Lagoon", Description = "Board motorized outrigger banca. Kayak through emerald waters of Big Lagoon between sheer limestone cliffs. Snorkel at Shimizu Island and relax on 7 Commandos Beach with grilled seafood lunch.", Meals = "Breakfast & Beach BBQ Lunch", ActivityHighlight = "Big Lagoon Kayaking & Snorkeling", Accommodation = "Pangulasian Island Resort, El Nido (5-Star Eco-Luxury Beachfront Villa)" },
                    new() { DayNumber = 3, Title = "Island Hopping Tour C: Secret Lagoon", Description = "Swim through a tiny keyhole in the cliff wall to enter Secret Lagoon. Discover the secluded white-sand crescent of Hidden Beach surrounded by dramatic limestone pinnacles.", Meals = "Breakfast & Beach BBQ Lunch", ActivityHighlight = "Secret Lagoon & Hidden Beach", Accommodation = "Pangulasian Island Resort, El Nido (5-Star Eco-Luxury Beachfront Villa)" },
                    new() { DayNumber = 4, Title = "Taraw Cliff Canopy Walk & Twin Beach", Description = "Morning suspension bridge walk across Taraw Cliff for sweeping bay views. Afternoon motorbike or tricycle ride to Twin Beach with coconut milkshakes.", Meals = "Breakfast", ActivityHighlight = "Taraw Cliff Canopy Walk", Accommodation = "Pangulasian Island Resort, El Nido (5-Star Eco-Luxury Beachfront Villa)" },
                    new() { DayNumber = 5, Title = "Souvenir Shopping & Flight Home", Description = "Morning beach swim and fresh mango breakfast. Pick up pearl souvenirs before private transfer to airport for departure.", Meals = "Breakfast", ActivityHighlight = "Palawan Pearl Souvenirs", Accommodation = "Check-out & El Nido Lio Airport Departure" }
                },
                Inclusions = new() { "4 Nights in Beachfront Resort", "Daily Breakfast & 2 Grilled Seafood Lunches on Beaches", "Island Hopping Tour A & Tour C with Licensed Guides & Snorkel Gear", "Eco-Tourism Development Fee (ETDF) & Lagoon Environmental Permits", "Private Airport Transfers" },
                Exclusions = new() { "Kayak rental fee inside Big Lagoon ($5 USD)", "International airfare to Philippines", "Personal expenses and dinners" }
            }
        };
    }
}
