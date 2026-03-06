using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAdmin.MVC.Models.Auth;

namespace WebAdmin.MVC.Controllers;

public class AccountController : Controller
{
    private const string LoginViewPath = "~/Views/Auth/Login.cshtml";

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(LoginViewPath, new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(LoginViewPath, vm);

        // Demo accounts
        string? role = null;

        if (vm.Email.Equals("admin@system.com", StringComparison.OrdinalIgnoreCase) && vm.Password == "Admin@123456")
            role = "Admin";
        else if (vm.Email.Equals("host@demo.com", StringComparison.OrdinalIgnoreCase) && vm.Password == "Host@123")
            role = "Host";

        if (role == null)
        {
            vm.ErrorMessage = "Invalid email or password.";
            return View(LoginViewPath, vm);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, vm.Email),
            new Claim(ClaimTypes.Name, vm.Email),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = vm.RememberMe });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Contracts");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
        => View("~/Views/Auth/AccessDenied.cshtml");
}