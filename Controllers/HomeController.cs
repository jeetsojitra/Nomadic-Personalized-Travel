using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nomadic.Models;
using Nomadic.Services;

namespace Nomadic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITravelDataService _travelDataService;

        public HomeController(ITravelDataService travelDataService)
        {
            _travelDataService = travelDataService;
        }

        public IActionResult Index()
        {
            ViewBag.Destinations = _travelDataService.GetAllDestinations();
            ViewBag.Packages = _travelDataService.GetAllPackages();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                });
        }
    }
}
