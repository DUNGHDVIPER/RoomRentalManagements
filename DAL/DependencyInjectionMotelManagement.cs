using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DAL;

public static class DependencyInjectionMotelManagement
{
    /// <summary>
    /// Register <see cref="MotelManagementDbContext"/> that maps to the provided MotelManagementDB schema.
    ///
    /// In appsettings.json:
    /// ConnectionStrings: { "MotelManagementDB": "Server=...;Database=MotelManagementDB;..." }
    /// </summary>
    public static IServiceCollection AddMotelManagementDb(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("MotelManagementDB")
                 ?? configuration.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Missing connection string: MotelManagementDB (or DefaultConnection)");

        services.AddDbContext<MotelManagementDbContext>(opt =>
            opt.UseSqlServer(cs, sql => sql.MigrationsAssembly(typeof(MotelManagementDbContext).Assembly.FullName)));

        return services;
    }
}
