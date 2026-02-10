using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebHostRazor;

public class AuthDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
}
