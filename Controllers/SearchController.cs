using Microsoft.AspNetCore.Mvc;
using Nomadic.Services;

namespace Nomadic.Controllers
{
    public class SearchController : Controller
    {
        private readonly ITravelDataService _travelDataService;

        public SearchController(ITravelDataService travelDataService)
        {
            _travelDataService = travelDataService;
        }

        [HttpGet]
        public IActionResult Index(
            string? destination,
            string? travelDate,
            string? budget,
            string? travelStyle)
        {
            ViewBag.Destination = destination;
            ViewBag.TravelDate = travelDate;
            ViewBag.Budget = budget;
            ViewBag.TravelStyle = travelStyle;
            ViewBag.Destinations = _travelDataService.GetAllDestinations();

            return View();
        }
    }
}
