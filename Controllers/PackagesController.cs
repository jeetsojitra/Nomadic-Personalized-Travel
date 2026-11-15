using Microsoft.AspNetCore.Mvc;
using Nomadic.Models;
using Nomadic.Services;

namespace Nomadic.Controllers
{
    public class PackagesController : Controller
    {
        private readonly ITravelDataService _travelDataService;

        public PackagesController(ITravelDataService travelDataService)
        {
            _travelDataService = travelDataService;
        }

        [HttpGet]
        public IActionResult Index(string? destination)
        {
            ViewBag.Destination = destination;
            ViewBag.AllDestinations = _travelDataService.GetAllDestinations();

            var packages = _travelDataService.GetPackagesByDestination(destination);
            return View(packages);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var package = _travelDataService.GetPackageById(id) 
                          ?? _travelDataService.GetAllPackages().FirstOrDefault();

            if (package == null)
            {
                return RedirectToAction(nameof(Index));
            }

            
            ViewBag.PackageId = package.Id;
            ViewBag.Name = package.Name;
            ViewBag.Destination = package.Destination;
            ViewBag.Price = package.FormattedPrice;
            ViewBag.Duration = package.Duration;
            ViewBag.Image = package.ImageUrl;
            ViewBag.Description = package.ShortDescription;

            return View(package);
        }
    }
}
