using Microsoft.AspNetCore.Mvc;
using Nomadic.Services;

namespace Nomadic.Controllers
{
    public class DestinationsController : Controller
    {
        private readonly ITravelDataService _travelDataService;

        public DestinationsController(ITravelDataService travelDataService)
        {
            _travelDataService = travelDataService;
        }

        [HttpGet]
        public IActionResult Index(string? category)
        {
            var destinations = _travelDataService.GetAllDestinations();

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", System.StringComparison.OrdinalIgnoreCase))
            {
                destinations = destinations.Where(d => d.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase)).ToList();
                ViewBag.ActiveCategory = category;
            }
            else
            {
                ViewBag.ActiveCategory = "All";
            }

            return View(destinations);
        }
    }
}
