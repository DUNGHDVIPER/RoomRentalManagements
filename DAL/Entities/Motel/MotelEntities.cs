using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities.Motel;

// -------------------------
// A) AUTH + RBAC + AUDIT
// -------------------------
public class User
{
    [Key] public int UserId { get; set; }
    [MaxLength(150)] public string Email { get; set; } = null!;
    [MaxLength(300)] public string? PasswordHash { get; set; }
    [MaxLength(150)] public string FullName { get; set; } = null;
    [MaxLength(30)] public string? Phone { get; set; }
    [MaxLength(500)] public string? AvatarUrl { get; set; }
    public bool IsLocked { get; set; }
    [MaxLength(255)] public string? LockReason { get; set; }
    [MaxLength(30)] public string? LoginProvider { get; set; }
    [MaxLength(200)] public string? ProviderUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Role
{
    [Key] public int RoleId { get; set; }
    [MaxLength(50)] public string RoleName { get; set; } = null!;
    [MaxLength(200)] public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

public class AuditLog
{
[Key]
    public long AuditLogId { get; set; }

    public int? ActorUserId { get; set; }                 // nullable (system jobs)

    [MaxLength(100)]
    public string Action { get; set; } = null!;           // e.g. "CreateContract"

    [MaxLength(100)]
    public string EntityType { get; set; } = null!;       // e.g. "Contract"

    [MaxLength(50)]
    public string EntityId { get; set; } = null!;         // e.g. "123"

    public DateTime CreatedAt { get; set; }               // UTC recommended

    [MaxLength(500)]
    public string? Note { get; set; }                     // short note

    public string? OldValueJson { get; set; }             // nvarchar(max)
    public string? NewValueJson { get; set; }             // nvarchar(max)

    // optional navigation if you have User entity
    public User? ActorUser { get; set; }
}

// -------------------------
// B) BLOCK - FLOOR - ROOM
// -------------------------
public class Block
{
    [Key] public int BlockId { get; set; }
    [MaxLength(100)] public string BlockName { get; set; } = null!;
    [MaxLength(255)] public string? Address { get; set; }
    [MaxLength(255)] public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}

public class Floor
{
    [Key] public int FloorId { get; set; }
    public int BlockId { get; set; }
    [MaxLength(50)] public string FloorName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Block Block { get; set; } = null!;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}

public class Room
{
    [Key] public int RoomId { get; set; }
    public int FloorId { get; set; }
    [MaxLength(50)] public string RoomCode { get; set; } = null!;
    [MaxLength(100)] public string? RoomName { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal? AreaM2 { get; set; }
    public int MaxOccupants { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Available";
    [Column(TypeName = "decimal(18,2)")] public decimal CurrentBasePrice { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public Floor Floor { get; set; } = null!;
    public ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();
    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
    public ICollection<RoomPricingHistory> PricingHistories { get; set; } = new List<RoomPricingHistory>();
}

public class RoomImage
{
    [Key] public int ImageId { get; set; }
    public int RoomId { get; set; }
    [MaxLength(500)] public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public Room Room { get; set; } = null!;
}

public class Amenity
{
    [Key] public int AmenityId { get; set; }
    [MaxLength(100)] public string AmenityName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}

public class RoomAmenity
{
    public int RoomId { get; set; }
    public int AmenityId { get; set; }
    public Room Room { get; set; } = null!;
    public Amenity Amenity { get; set; } = null!;
}

public class RoomPricingHistory
{
    [Key] public long PriceId { get; set; }
    public int RoomId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal OldPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
    public int? ChangedByUserId { get; set; }
    [MaxLength(255)] public string? Note { get; set; }

    public Room Room { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}

// -------------------------
// C) TENANTS + DOCS + RESIDENTS + BLACKLIST
// -------------------------
public class Tenant
{
    [Key] public int TenantId { get; set; }
    [MaxLength(150)] public string FullName { get; set; } = null!;
    [MaxLength(30)] public string Phone { get; set; } = null!;
    [MaxLength(150)] public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [MaxLength(10)] public string? Gender { get; set; }
    [MaxLength(255)] public string? CurrentAddress { get; set; }
    public bool IsBlacklisted { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<TenantDocument> Documents { get; set; } = new List<TenantDocument>();
    public ICollection<RoomResident> RoomResidents { get; set; } = new List<RoomResident>();
}

public class TenantDocument
{
    [Key] public int DocumentId { get; set; }
    public int TenantId { get; set; }
    [MaxLength(50)] public string DocType { get; set; } = null!;
    [MaxLength(50)] public string? DocNumber { get; set; }
    [MaxLength(500)] public string? FrontImageUrl { get; set; }
    [MaxLength(500)] public string? BackImageUrl { get; set; }
    public DateTime? IssuedDate { get; set; }
    [MaxLength(100)] public string? IssuedPlace { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}

public class RoomResident
{
    [Key] public long ResidentId { get; set; }
    public int RoomId { get; set; }
    public int TenantId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public bool IsActive { get; set; }

    public Room Room { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}

public class TenantBlacklist
{
    [Key] public int BlacklistId { get; set; }
    public int TenantId { get; set; }
    [MaxLength(255)] public string Reason { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}

// -------------------------
// D) CONTRACTS
// -------------------------
public class Contract
{
    [Key] public long ContractId { get; set; }
    public int RoomId { get; set; }
    public int TenantId { get; set; }
    [MaxLength(50)] public string ContractCode { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal BaseRent { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal DepositAmount { get; set; }
    [MaxLength(20)] public string PaymentCycle { get; set; } = "Monthly";
    [MaxLength(20)] public string Status { get; set; } = "Active";
    [MaxLength(500)] public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }

    public Room Room { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<ContractAttachment> Attachments { get; set; } = new List<ContractAttachment>();
    public ICollection<ContractVersion> Versions { get; set; } = new List<ContractVersion>();
    public ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
    [MaxLength(20)]
    public string DepositStatus { get; set; } = "Unpaid"; // Unpaid/Paid/Refunded/Forfeit

    public DateTime? DepositPaidAt { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DepositPaidAmount { get; set; }
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
}

public class ContractAttachment
{
    [Key] public long AttachmentId { get; set; }
    public long ContractId { get; set; }
    [MaxLength(500)] public string FileUrl { get; set; } = null!;
    [MaxLength(200)] public string? FileName { get; set; }
    public DateTime UploadedAt { get; set; }
    public int? UploadedByUserId { get; set; }
    public Contract Contract { get; set; } = null!;
    public User? UploadedByUser { get; set; }
}

public class ContractVersion
{
    [Key] public long VersionId { get; set; }
    public long ContractId { get; set; }
    public int VersionNumber { get; set; }
    public DateTime ChangedAt { get; set; }
    public int? ChangedByUserId { get; set; }
    [MaxLength(255)] public string? ChangeNote { get; set; }
    public string SnapshotJson { get; set; } = null!;

    public Contract Contract { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}

public class Deposit
{
    [Key] public long DepositId { get; set; }
    public long ContractId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [MaxLength(30)] public string Type { get; set; } = "Hold";
    [MaxLength(255)] public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public Contract Contract { get; set; } = null!;
}
public class ContractReminderLog
{
    [Key]
    public long Id { get; set; }

    public long ContractId { get; set; }

    [MaxLength(30)]
    public string RemindType { get; set; } = null!; // e.g. "Expiry_7d"

    // Chốt theo "ngày" để chống trùng trong ngày
    [Column(TypeName = "date")]
    public DateTime RemindAtDate { get; set; }

    public DateTime CreatedAt { get; set; }

    // optional nav
    public Contract Contract { get; set; } = null!;
}

// -------------------------
// E) MAINTENANCE + NOTIFICATIONS
// -------------------------
public class MaintenanceTicket
{
    [Key] public long TicketId { get; set; }
    public int RoomId { get; set; }
    public int? TenantId { get; set; }
    [MaxLength(200)] public string Title { get; set; } = null!;
    [MaxLength(1000)] public string? Description { get; set; }
    [MaxLength(20)] public string Priority { get; set; } = "Medium";
    [MaxLength(20)] public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; }
    public Room Room { get; set; } = null!;
    public Tenant? Tenant { get; set; }
    public ICollection<TicketAssignment> Assignments { get; set; } = new List<TicketAssignment>();
}

public class TicketAssignment
{
    [Key] public long AssignmentId { get; set; }
    public long TicketId { get; set; }
    public int AssignedToUserId { get; set; }
    public DateTime AssignedAt { get; set; }
    [MaxLength(255)] public string? Note { get; set; }
    public MaintenanceTicket Ticket { get; set; } = null!;
    public User AssignedToUser { get; set; } = null!;
}

public class Notification
{
    [Key] public long NotificationId { get; set; }
    public int CreatedByUserId { get; set; }
    [MaxLength(200)] public string Title { get; set; } = null!;
    [MaxLength(1000)] public string Message { get; set; } = null!;
    [MaxLength(30)] public string Type { get; set; } = "Broadcast";
    public DateTime CreatedAt { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
}

public class NotificationRecipient
{
    [Key] public long RecipientId { get; set; }
    public long NotificationId { get; set; }
    public int TenantId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public Notification Notification { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}

// -------------------------
// F) UTILITIES + BILLS + PAYMENTS
// -------------------------
public class UtilityPricing
{
    [Key] public int UtilityPricingId { get; set; }
    public int? BlockId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ElectricityPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal WaterPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ServiceFee { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public Block? Block { get; set; }
}

public class UtilityReading
{
    [Key] public long ReadingId { get; set; }
    public int RoomId { get; set; }
    [MaxLength(7)] public string MonthKey { get; set; } = null!; // YYYY-MM
    public int ElectricityOld { get; set; }
    public int ElectricityNew { get; set; }
    public int WaterOld { get; set; }
    public int WaterNew { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public Room Room { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}

public class ExtraFee
{
    [Key] public int FeeId { get; set; }
    [MaxLength(150)] public string FeeName { get; set; } = null!;
    [Column(TypeName = "decimal(18,2)")] public decimal DefaultAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Bill
{
    [Key] public long BillId { get; set; }
    public int RoomId { get; set; }
    public int TenantId { get; set; }
    public long? ContractId { get; set; }
    [MaxLength(7)] public string MonthKey { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")] public decimal BaseRent { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ElectricityCharge { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal WaterCharge { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ServiceFee { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ExtraFeeTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PaidAmount { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Unpaid";
    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }

    public Room Room { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public Contract? Contract { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<BillItem> Items { get; set; } = new List<BillItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<BillStatusHistory> StatusHistories { get; set; } = new List<BillStatusHistory>();
}

public class BillItem
{
    [Key] public long BillItemId { get; set; }
    public long BillId { get; set; }
    [MaxLength(200)] public string ItemName { get; set; } = null!;
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    public Bill Bill { get; set; } = null!;
}

public class Payment
{
    [Key] public long PaymentId { get; set; }
    public long BillId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [MaxLength(20)] public string Method { get; set; } = null!;
    public DateTime PaidAt { get; set; }
    [MaxLength(255)] public string? Note { get; set; }
    public int? CreatedByUserId { get; set; }
    public Bill Bill { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}

public class BillStatusHistory
{
    [Key] public long HistoryId { get; set; }
    public long BillId { get; set; }
    [MaxLength(20)] public string OldStatus { get; set; } = null!;
    [MaxLength(20)] public string NewStatus { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public int? ChangedByUserId { get; set; }
    public Bill Bill { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}

// -------------------------
// G) BOOKINGS
// -------------------------
public class Booking
{
    [Key] public long BookingId { get; set; }
    public int RoomId { get; set; }
    public int TenantId { get; set; }
    public DateTime? DesiredMoveInDate { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public Room Room { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
