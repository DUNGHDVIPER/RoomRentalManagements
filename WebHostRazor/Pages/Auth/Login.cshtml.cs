using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebHostRazor.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;

    public LoginModel(SignInManager<IdentityUser> signIn) => _signIn = signIn;

    [BindProperty] public string Email { get; set; } = "host@demo.com";
    [BindProperty] public string Password { get; set; } = "Host@123";
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        Error = null;
        var res = await _signIn.PasswordSignInAsync(Email, Password, true, false);
        if (res.Succeeded) return Redirect("/Host/Dashboard");
        Error = "Invalid email/password.";
        return Page();
    }
}
