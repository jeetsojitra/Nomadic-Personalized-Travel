using Microsoft.AspNetCore.Mvc;

namespace Nomadic.Controllers
{
    public class SubscriptionController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Index(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Message =
                    "Please enter your email address.";

                return View();
            }

            ViewBag.Message =
                "Thank you for subscribing to Nomadic!";

            return View();
        }
    }
}