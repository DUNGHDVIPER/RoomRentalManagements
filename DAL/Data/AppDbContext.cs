using DAL.Entities.Billing;
using DAL.Entities.Contracts;
using DAL.Entities.Motel;
using DAL.Entities.System;
using DAL.Entities.Tenanting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using AmenityEntity = DAL.Entities.Property.Amenity;
using BlockEntity = DAL.Entities.Property.Block;
using FloorEntity = DAL.Entities.Property.Floor;
using RoomAmenityEntity = DAL.Entities.Property.RoomAmenity;
using RoomEntity = DAL.Entities.Property.Room;
using RoomImageEntity = DAL.Entities.Property.RoomImage;
using RoomPricingHistoryEntity = DAL.Entities.Property.RoomPricingHistory;

namespace DAL.Data;

public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // =========================
    // SYSTEM
    // =========================
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>();

    // =========================
    // PROPERTY
    // =========================
    public DbSet<BlockEntity> Blocks => Set<BlockEntity>();
    public DbSet<FloorEntity> Floors => Set<FloorEntity>();
    public DbSet<RoomEntity> Rooms => Set<RoomEntity>();
    public DbSet<AmenityEntity> Amenities => Set<AmenityEntity>();
    public DbSet<RoomAmenityEntity> RoomAmenities => Set<RoomAmenityEntity>();
    public DbSet<RoomImageEntity> RoomImages => Set<RoomImageEntity>();
    public DbSet<RoomPricingHistoryEntity> RoomPricingHistories => Set<RoomPricingHistoryEntity>();

    // =========================
    // TENANTING
    // =========================
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantIdDoc> TenantIdDocs => Set<TenantIdDoc>();
    public DbSet<StayHistory> StayHistories => Set<StayHistory>();
    public DbSet<RoomResident> RoomResidents => Set<RoomResident>();

    // =========================
    // CONTRACTS
    // =========================
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractAttachment> ContractAttachments => Set<ContractAttachment>();
    public DbSet<ContractVersion> ContractVersions => Set<ContractVersion>();
    public DbSet<Deposit> Deposits => Set<Deposit>();
    public DbSet<ContractReminder> ContractReminders => Set<ContractReminder>();
    public DbSet<ContractReminderLog> ContractReminderLogs => Set<ContractReminderLog>();

    // =========================
    // BILLING
    // =========================
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillItem> BillItems => Set<BillItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BillStatusHistory> BillStatusHistories => Set<BillStatusHistory>();

    // =========================
    // MAINTENANCE
    // =========================
    public DbSet<DAL.Entities.Maintenance.Ticket> Tickets => Set<DAL.Entities.Maintenance.Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Composite key RoomAmenity
        modelBuilder.Entity<RoomAmenityEntity>()
            .HasKey(x => new { x.RoomId, x.AmenityId });

        // Load entity configuration automatically
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Map Room -> table Rooms
        modelBuilder.Entity<RoomEntity>()
            .ToTable("Rooms", "dbo");

        // =========================
        // CONTRACT FIX
        // =========================
        modelBuilder.Entity<Contract>(e =>
        {
            e.Ignore(x => x.Id);
            e.HasKey(x => x.ContractId);

            e.Property(x => x.ContractId)
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ContractVersion>(e =>
        {
            e.HasKey(x => x.VersionId);

            e.Property(x => x.VersionId)
                .ValueGeneratedOnAdd();
        });

        // =========================
        // AUDIT LOG FIX
        // =========================
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<AuditLog>()
            .Property(x => x.OldValueJson)
            .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<AuditLog>()
            .Property(x => x.NewValueJson)
            .HasColumnType("nvarchar(max)");

        // =========================
        // USER ROLE RELATION
        // =========================
        modelBuilder.Entity<UserRole>()
            .HasKey(x => new { x.UserId, x.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(x => x.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(x => x.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(x => x.RoleId);

        // =========================
        // UTILITY READING PRECISION
        // =========================
        modelBuilder.Entity<UtilityReading>(e =>
        {
            e.Property(x => x.ElectricKwh).HasPrecision(18, 3);
            e.Property(x => x.WaterM3).HasPrecision(18, 3);
        });
    }
}