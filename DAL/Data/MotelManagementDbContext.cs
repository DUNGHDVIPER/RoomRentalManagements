using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;




namespace DAL.Data;

public class MotelManagementDbContext : DbContext
{
    public MotelManagementDbContext(DbContextOptions<MotelManagementDbContext> options) : base(options) { }

    // A) Auth/RBAC/Audit
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // B) Block/Floor/Room
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<RoomAmenity> RoomAmenities => Set<RoomAmenity>();

    public DbSet<RoomImage> RoomImages => Set<RoomImage>();
    
    public DbSet<RoomPricingHistory> RoomPricingHistories => Set<RoomPricingHistory>();

    // C) Tenants + docs + residents + blacklist
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantDocument> TenantDocuments => Set<TenantDocument>();
    public DbSet<RoomResident> RoomResidents => Set<RoomResident>();
    public DbSet<TenantBlacklist> TenantBlacklists => Set<TenantBlacklist>();

    // D) Contracts
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractAttachment> ContractAttachments => Set<ContractAttachment>();
    public DbSet<ContractVersion> ContractVersions => Set<ContractVersion>();
    public DbSet<Deposit> Deposits => Set<Deposit>();

    // E) Maintenance + notifications
    public DbSet<MaintenanceTicket> MaintenanceTickets => Set<MaintenanceTicket>();
    public DbSet<TicketAssignment> TicketAssignments => Set<TicketAssignment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>();

    // F) Utilities/Bills/Payments/Report
    public DbSet<UtilityPricing> UtilityPricings => Set<UtilityPricing>();
    public DbSet<UtilityReading> UtilityReadings => Set<UtilityReading>();
    public DbSet<ExtraFee> ExtraFees => Set<ExtraFee>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillItem> BillItems => Set<BillItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BillStatusHistory> BillStatusHistories => Set<BillStatusHistory>();
    public DbSet<ContractReminderLog> ContractReminderLogs => Set<ContractReminderLog>();
  
    // G) Bookings
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // các entity join table trong MotelEntities.cs
        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<RoomAmenity>().HasKey(x => new { x.RoomId, x.AmenityId });

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MotelManagementDbContext).Assembly,
            t => t.Namespace != null && t.Namespace.Contains("DAL.Data.ConfigurationsMotel")
        );
        modelBuilder.Entity<AuditLog>()
    .Property(x => x.OldValueJson)
    .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<AuditLog>()
            .Property(x => x.NewValueJson)
            .HasColumnType("nvarchar(max)");
        modelBuilder.Entity<AuditLog>()
    .Property(x => x.OldValueJson)
    .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<AuditLog>()
            .Property(x => x.NewValueJson)
            .HasColumnType("nvarchar(max)");
    }



}

public class MotelManagementDbContextFactory : IDesignTimeDbContextFactory<MotelManagementDbContext>
{
    public MotelManagementDbContext CreateDbContext(string[] args)
    {
        // 1) Try read connection string from WebHostRazor/appsettings*.json
        var basePath = Directory.GetCurrentDirectory();

        // Bạn hay chạy lệnh trong folder DAL, nên WebHostRazor thường nằm ../WebHostRazor
        var webHostPath = Path.GetFullPath(Path.Combine(basePath, "..", "WebHostRazor"));
        if (!Directory.Exists(webHostPath))
        {
            // Nếu bạn chạy lệnh ở root solution, thử ./WebHostRazor
            webHostPath = Path.GetFullPath(Path.Combine(basePath, "WebHostRazor"));
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(webHostPath) ? webHostPath : basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Đổi key cho đúng với project bạn (DefaultConnection / ConnectionStrings:MotelManagementDB ...)
        var cs =
            config.GetConnectionString("DefaultConnection")
            ?? config.GetConnectionString("MotelManagementDB")
            ?? config["ConnectionStrings:DefaultConnection"]
            ?? config["ConnectionStrings:MotelManagementDB"];

        // 2) Fallback hardcode (điền đúng theo máy bạn)
        if (string.IsNullOrWhiteSpace(cs))
        {
            cs = "Server=ADMIN\\SQL2022;Database=MotelManagementDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
        }

        var optionsBuilder = new DbContextOptionsBuilder<MotelManagementDbContext>();
        optionsBuilder.UseSqlServer(cs);

        return new MotelManagementDbContext(optionsBuilder.Options);
    }
}


