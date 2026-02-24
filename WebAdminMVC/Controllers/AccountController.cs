using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAdmin.MVC.Models.Auth;

namespace WebAdmin.MVC.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        // ✅ Demo: hard-code user (bạn thay bằng check DB sau)
        // role phải là Admin hoặc Host để vào ContractsController [Authorize(Roles="Admin,Host")]
        var ok =
            (vm.Username == "admin" && vm.Password == "123" && (vm.Role == "Admin" || vm.Role == "Host")) ||
            (vm.Username == "host" && vm.Password == "123" && vm.Role == "Host");

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Invalid username/password/role.");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, vm.Username),
            new Claim(ClaimTypes.Name, vm.Username),
            new Claim(ClaimTypes.Role, vm.Role) // VERY IMPORTANT cho [Authorize(Roles="...")]
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = vm.RememberMe,
                RedirectUri = vm.ReturnUrl
            });

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

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
    {
        return View();
    }
}