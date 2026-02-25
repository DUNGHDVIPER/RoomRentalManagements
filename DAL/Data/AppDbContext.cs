/*using DAL.Entities.Billing;
using DAL.Entities.Contracts;
using DAL.Entities.Maintenance;
using DAL.Entities.Motel;
using DAL.Entities.System;
using DAL.Entities.Tenanting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Property
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomImage> RoomImages => Set<RoomImage>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<RoomAmenity> RoomAmenities => Set<RoomAmenity>();
    public DbSet<RoomPriceHistory> RoomPriceHistories => Set<RoomPriceHistory>();

    // Tenanting
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantIdDoc> TenantIdDocs => Set<TenantIdDoc>();
    public DbSet<StayHistory> StayHistories => Set<StayHistory>();

    // Maintenance
    public DbSet<Ticket> Tickets => Set<Ticket>();

    // Contracts
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractVersion> ContractVersions => Set<ContractVersion>();
    public DbSet<ContractReminder> ContractReminders => Set<ContractReminder>();

    // Billing
    public DbSet<UtilityPrice> UtilityPrices => Set<UtilityPrice>();
    public DbSet<UtilityReading> UtilityReadings => Set<UtilityReading>();
    public DbSet<ExtraFee> ExtraFees => Set<ExtraFee>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillItem> BillItems => Set<BillItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    // System
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration in this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
*/