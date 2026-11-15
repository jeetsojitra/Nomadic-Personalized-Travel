using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nomadic.Data;

namespace Nomadic.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<NomadicUser> _userManager;
    private readonly SignInManager<NomadicUser> _signInManager;

    public AccountController(UserManager<NomadicUser> userManager, SignInManager<NomadicUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool remember = false, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Message = "Please enter email and password.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(email.Trim(), password, remember, lockoutOnFailure: true);
        if (result.Succeeded)
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Index", "Home");

        ViewBag.Message = result.IsLockedOut
            ? "Your account is temporarily locked. Please try again later."
            : "Invalid email or password.";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Message = "Please fill all fields.";
            return View();
        }
        if (!new EmailAddressAttribute().IsValid(email))
        {
            ViewBag.Message = "Please enter a valid email address.";
            return View();
        }
        if (password != confirmPassword)
        {
            ViewBag.Message = "Passwords do not match.";
            return View();
        }

        var user = new NomadicUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            FullName = name.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            ViewBag.Message = string.Join(" ", result.Errors.Select(x => x.Description));
            return View();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View("Login");
}
