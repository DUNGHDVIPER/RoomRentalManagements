using System.Data;
using System.Text.Json;
using BLL.Common;
using BLL.DTOs.Common;
using BLL.DTOs.Contract;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ContractService : IContractService
{
    private readonly MotelManagementDbContext _db;
    private readonly IAuditService _audit;

    public ContractService(MotelManagementDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    // =========================
    // Centralized status constants
    // =========================
    private static class ContractStatus
    {
        public const string Pending = "Pending";
        public const string Active = "Active";
        public const string Expired = "Expired";
        public const string Terminated = "Terminated";
        public const string Renewed = "Renewed";

        public static bool Is(string? s, string expected)
            => string.Equals(s?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
    }

    // =========================
    // 1) CREATE
    // =========================
    public async Task<ContractDto> CreateAsync(CreateContractDto dto, int? actorUserId = null, CancellationToken ct = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        ValidateDateRange(dto.StartDate, dto.EndDate, "StartDate", "EndDate");

        if (dto.RoomId <= 0) throw new InvalidOperationException("RoomId is missing.");
        if (dto.TenantId <= 0) throw new InvalidOperationException("TenantId is missing.");

        // lightweight validate existence
        if (!await _db.Rooms.AsNoTracking().AnyAsync(r => r.RoomId == dto.RoomId, ct))
            throw new InvalidOperationException("Room not found.");
        if (!await _db.Tenants.AsNoTracking().AnyAsync(t => t.TenantId == dto.TenantId, ct))
            throw new InvalidOperationException("Tenant not found.");

        // Pre-check message (friendly)
        if (dto.ActivateNow)
        {
            var hasActive = await _db.Contracts.AsNoTracking()
                .AnyAsync(c => c.RoomId == dto.RoomId && c.Status == ContractStatus.Active, ct);

            if (hasActive)
                throw new InvalidOperationException(
                    $"Cannot activate contract: Room {dto.RoomId} already has an ACTIVE contract. " +
                    $"Please renew/terminate the current active contract first.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var status = dto.ActivateNow ? ContractStatus.Active : ContractStatus.Pending;
            var code = await GenerateContractCodeAsync(ct);

            var contract = new Contract
            {
                RoomId = dto.RoomId,
                TenantId = dto.TenantId,
                ContractCode = code,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                BaseRent = dto.Rent,
                DepositAmount = dto.Deposit,
                PaymentCycle = "Monthly",
                Status = status,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = actorUserId
            };

            _db.Contracts.Add(contract);

            if (dto.Deposit > 0)
            {
                _db.Deposits.Add(new Deposit
                {
                    Contract = contract,
                    Amount = dto.Deposit,
                    Type = "Hold",
                    Note = "Initial deposit",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);

            await CreateVersionSnapshotInternalAsync(contract.ContractId, actorUserId, "Create contract", ct);

            await _audit.LogAsync(
                actorUserId,
                action: "CreateContract",
                entityType: "Contract",
                entityId: contract.ContractId.ToString(),
                note: "Create contract",
                oldValue: null,
                newValue: new
                {
                    contract.ContractId,
                    contract.ContractCode,
                    contract.RoomId,
                    contract.TenantId,
                    contract.StartDate,
                    contract.EndDate,
                    contract.BaseRent,
                    contract.DepositAmount,
                    contract.PaymentCycle,
                    contract.Status
                },
                ct: ct);

            await tx.CommitAsync(ct);
            return MapToDto(contract);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation() && ex.IsOneActivePerRoomViolation())
        {
            await tx.RollbackAsync(ct);
            throw new InvalidOperationException(
                "Cannot activate contract because this room already has an ACTIVE contract (DB constraint). " +
                "Please renew/terminate the existing active contract and try again.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // =========================
    // 2) RENEW
    // =========================
    public async Task<ContractDto> RenewAsync(RenewContractDto dto, int? actorUserId = null, CancellationToken ct = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        ValidateDateRange(dto.NewStartDate, dto.NewEndDate, "NewStartDate", "NewEndDate");

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var old = await _db.Contracts
                .FirstOrDefaultAsync(c => c.ContractId == (long)dto.ContractId, ct);

            if (old == null) throw new InvalidOperationException("Contract not found.");

            EnsureActive(old, "renew");

            // rule: new start must be >= old.EndDate or EndDate+1 depending flag
            var minStart = dto.RequireStartNextDay ? old.EndDate.Date.AddDays(1) : old.EndDate.Date;
            if (dto.NewStartDate.Date < minStart)
                throw new InvalidOperationException($"NewStartDate must be >= {minStart:yyyy-MM-dd}.");

            // Ensure no other active in room
            var hasOtherActive = await _db.Contracts.AsNoTracking()
                .AnyAsync(c => c.RoomId == old.RoomId
                            && c.ContractId != old.ContractId
                            && c.Status == ContractStatus.Active, ct);

            if (hasOtherActive)
                throw new InvalidOperationException("Renew failed: Room already has another ACTIVE contract.");

            // Snapshot before changes
            await CreateVersionSnapshotInternalAsync(old.ContractId, actorUserId, "Before renew (old)", ct);

            var oldAudit = new
            {
                old.ContractId,
                old.ContractCode,
                old.Status,
                old.StartDate,
                old.EndDate,
                old.BaseRent,
                old.DepositAmount,
                old.Note
            };

            // Old -> Renewed
            old.Status = ContractStatus.Renewed;
            old.Note = AppendNote(old.Note,
                $"Renewed -> new period {dto.NewStartDate:yyyy-MM-dd} to {dto.NewEndDate:yyyy-MM-dd}. Reason: {dto.Reason}");

            // New contract -> Active
            var newCode = await GenerateContractCodeAsync(ct);

            var renewed = new Contract
            {
                RoomId = old.RoomId,
                TenantId = old.TenantId,
                ContractCode = newCode,
                StartDate = dto.NewStartDate.Date,
                EndDate = dto.NewEndDate.Date,
                BaseRent = dto.NewRent,
                DepositAmount = dto.NewDeposit,
                PaymentCycle = old.PaymentCycle,
                Status = ContractStatus.Active,
                Note = $"Renew from {old.ContractCode}. Reason: {dto.Reason}",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = actorUserId
            };

            _db.Contracts.Add(renewed);

            await _db.SaveChangesAsync(ct);

            await CreateVersionSnapshotInternalAsync(old.ContractId, actorUserId, "After renew (old renewed)", ct);
            await CreateVersionSnapshotInternalAsync(renewed.ContractId, actorUserId, "After renew (new created)", ct);

            await _audit.LogAsync(
                actorUserId,
                action: "RenewContract",
                entityType: "Contract",
                entityId: old.ContractId.ToString(),
                note: $"NewContractId={renewed.ContractId}",
                oldValue: oldAudit,
                newValue: new
                {
                    renewed.ContractId,
                    renewed.ContractCode,
                    renewed.Status,
                    renewed.StartDate,
                    renewed.EndDate,
                    renewed.BaseRent,
                    renewed.DepositAmount
                },
                ct: ct);

            await tx.CommitAsync(ct);
            return MapToDto(renewed);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation() && ex.IsOneActivePerRoomViolation())
        {
            await tx.RollbackAsync(ct);
            throw new InvalidOperationException(
                "Renew failed because the room already has another ACTIVE contract (DB constraint).");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // =========================
    // 3) TERMINATE
    // =========================
    public async Task TerminateAsync(TerminateContractDto dto, int? actorUserId = null, CancellationToken ct = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        var terminateDate = dto.TerminateDate.Date;

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var c = await _db.Contracts
                .FirstOrDefaultAsync(x => x.ContractId == (long)dto.ContractId, ct);

            if (c == null) throw new InvalidOperationException("Contract not found.");

            EnsureActive(c, "terminate");

            if (terminateDate < c.StartDate.Date || terminateDate > c.EndDate.Date)
                throw new InvalidOperationException("TerminateDate must be within [StartDate, EndDate].");

            await CreateVersionSnapshotInternalAsync(c.ContractId, actorUserId, "Before terminate", ct);

            var oldAudit = new { c.Status, c.EndDate, c.Note };

            // IMPORTANT: save endDateBefore BEFORE changing
            var endDateBefore = c.EndDate.Date;

            c.Status = ContractStatus.Terminated;
            c.EndDate = terminateDate;
            c.Note = AppendNote(c.Note, $"Terminated at {terminateDate:yyyy-MM-dd}. Reason: {dto.Reason}");

            // RoomResidents checkout
            var residents = await _db.RoomResidents
                .Where(r => r.RoomId == c.RoomId && r.TenantId == c.TenantId && r.IsActive)
                .ToListAsync(ct);

            foreach (var r in residents)
            {
                r.CheckOutDate = terminateDate;
                r.IsActive = false;
            }

            await _db.SaveChangesAsync(ct);

            await CreateVersionSnapshotInternalAsync(c.ContractId, actorUserId, "After terminate", ct);

            await _audit.LogAsync(
                actorUserId,
                action: "TerminateContract",
                entityType: "Contract",
                entityId: c.ContractId.ToString(),
                note: dto.Reason,
                oldValue: new { oldAudit.Status, EndDate = endDateBefore, oldAudit.Note },
                newValue: new { c.Status, c.EndDate, c.Note, TerminatedAt = terminateDate },
                ct: ct);

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // =========================
    // 4) GET LIST + GET BY ID
    // =========================
    public async Task<PagedResultDto<ContractDto>> GetContractsAsync(PagedRequestDto req, CancellationToken ct = default)
    {
        var q = _db.Contracts.AsNoTracking();

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(c => c.ContractId)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(c => new ContractDto
            {
                Id = (int)c.ContractId,
                RoomId = c.RoomId,
                TenantId = c.TenantId,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Rent = c.BaseRent,
                Deposit = c.DepositAmount,
                Status = StatusToInt(c.Status),
                IsActive = c.Status == ContractStatus.Active,
                ContractCode = c.ContractCode
            })
            .ToListAsync(ct);

        return new PagedResultDto<ContractDto>(items, total, req.Page, req.PageSize);
    }

    public async Task<ContractDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var contractId = (long)id;

        var c = await _db.Contracts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ContractId == contractId, ct);

        if (c == null) throw new InvalidOperationException("Contract not found.");
        return MapToDto(c);
    }

    // =========================
    // 5) DEPOSIT MANAGEMENT
    // =========================
    public async Task UpdateDepositAsync(int contractId, decimal newDeposit, int? actorUserId = null, CancellationToken ct = default)
    {
        var id = (long)contractId;

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var contract = await _db.Contracts.FirstOrDefaultAsync(c => c.ContractId == id, ct);
            if (contract == null) throw new InvalidOperationException("Contract not found.");

            await CreateVersionSnapshotInternalAsync(contract.ContractId, actorUserId, "Before deposit update", ct);

            var oldDeposit = contract.DepositAmount;
            contract.DepositAmount = newDeposit;

            if (newDeposit != oldDeposit)
            {
                _db.Deposits.Add(new Deposit
                {
                    ContractId = contract.ContractId,
                    Amount = newDeposit - oldDeposit,
                    Type = "Hold",
                    Note = $"Deposit changed {oldDeposit} -> {newDeposit}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);

            await CreateVersionSnapshotInternalAsync(contract.ContractId, actorUserId, "After deposit update", ct);

            await _audit.LogAsync(
                actorUserId,
                action: "UpdateDeposit",
                entityType: "Contract",
                entityId: contract.ContractId.ToString(),
                note: "Update deposit amount",
                oldValue: new { DepositAmount = oldDeposit },
                newValue: new { DepositAmount = contract.DepositAmount },
                ct: ct);

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task UpdateDepositAsync(UpdateDepositDto dto, int? actorUserId = null, CancellationToken ct = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        dto.DepositStatus = (dto.DepositStatus ?? "Unpaid").Trim();

        static bool IsOneOf(string s, params string[] vals)
            => vals.Any(v => string.Equals(s, v, StringComparison.OrdinalIgnoreCase));

        if (!IsOneOf(dto.DepositStatus, "Unpaid", "Paid", "Refunded", "Forfeit"))
            throw new InvalidOperationException("DepositStatus invalid. Use Unpaid/Paid/Refunded/Forfeit.");

        if (string.Equals(dto.DepositStatus, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            if (dto.PaidAt == null) throw new InvalidOperationException("PaidAt is required when DepositStatus=Paid.");
            if ((dto.PaidAmount ?? 0) <= 0) throw new InvalidOperationException("PaidAmount must be > 0 when DepositStatus=Paid.");
        }

        if (string.Equals(dto.DepositStatus, "Unpaid", StringComparison.OrdinalIgnoreCase))
        {
            if (dto.PaidAt != null) throw new InvalidOperationException("PaidAt must be null when DepositStatus=Unpaid.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var c = await _db.Contracts.FirstOrDefaultAsync(x => x.ContractId == (long)dto.ContractId, ct);
            if (c == null) throw new InvalidOperationException("Contract not found.");

            await CreateVersionSnapshotInternalAsync(c.ContractId, actorUserId, "Before deposit update", ct);

            var oldAudit = new
            {
                c.DepositAmount,
                c.DepositStatus,
                c.DepositPaidAt,
                c.DepositPaidAmount
            };

            var oldDepositAmount = c.DepositAmount;
            var oldStatus = c.DepositStatus;
            var oldPaidAmount = c.DepositPaidAmount;

            c.DepositAmount = dto.DepositAmount;
            c.DepositStatus = dto.DepositStatus;
            c.DepositPaidAt = dto.PaidAt?.ToUniversalTime();
            c.DepositPaidAmount = dto.PaidAmount;

            if (string.Equals(c.DepositStatus, "Unpaid", StringComparison.OrdinalIgnoreCase))
            {
                c.DepositPaidAt = null;
                c.DepositPaidAmount = null;
            }

            // deposit history rows
            var delta = c.DepositAmount - oldDepositAmount;
            if (delta != 0)
            {
                _db.Deposits.Add(new Deposit
                {
                    ContractId = c.ContractId,
                    Amount = delta,
                    Type = "Hold",
                    Note = $"DepositAmount changed {oldDepositAmount} -> {c.DepositAmount}. {dto.Note}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!string.Equals(oldStatus, c.DepositStatus, StringComparison.OrdinalIgnoreCase))
            {
                var t = c.DepositStatus switch
                {
                    "Paid" => "Paid",
                    "Refunded" => "Refund",
                    "Forfeit" => "Forfeit",
                    _ => "Hold"
                };

                var amount = (c.DepositPaidAmount ?? 0);
                if (string.Equals(c.DepositStatus, "Paid", StringComparison.OrdinalIgnoreCase) && amount <= 0)
                    amount = c.DepositAmount;

                _db.Deposits.Add(new Deposit
                {
                    ContractId = c.ContractId,
                    Amount = amount,
                    Type = t,
                    Note = $"DepositStatus {oldStatus} -> {c.DepositStatus}. {dto.Note}",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                if (string.Equals(c.DepositStatus, "Paid", StringComparison.OrdinalIgnoreCase)
                    && (oldPaidAmount ?? 0) != (c.DepositPaidAmount ?? 0))
                {
                    _db.Deposits.Add(new Deposit
                    {
                        ContractId = c.ContractId,
                        Amount = (c.DepositPaidAmount ?? 0) - (oldPaidAmount ?? 0),
                        Type = "Paid",
                        Note = $"PaidAmount changed {(oldPaidAmount ?? 0)} -> {(c.DepositPaidAmount ?? 0)}. {dto.Note}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync(ct);

            await CreateVersionSnapshotInternalAsync(c.ContractId, actorUserId, "After deposit update", ct);

            await _audit.LogAsync(
                actorUserId,
                action: "UpdateDeposit",
                entityType: "Contract",
                entityId: c.ContractId.ToString(),
                note: dto.Note,
                oldValue: oldAudit,
                newValue: new
                {
                    c.DepositAmount,
                    c.DepositStatus,
                    c.DepositPaidAt,
                    c.DepositPaidAmount
                },
                ct: ct);

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // =========================
    // 6) ATTACHMENT UPLOAD (metadata)
    // =========================
    public async Task AddAttachmentStubAsync(int contractId, string fileName, string url, int? actorUserId = null, CancellationToken ct = default)
    {
        var id = (long)contractId;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var contractExists = await _db.Contracts.AsNoTracking().AnyAsync(c => c.ContractId == id, ct);
            if (!contractExists) throw new InvalidOperationException("Contract not found.");

            _db.ContractAttachments.Add(new ContractAttachment
            {
                ContractId = id,
                FileName = fileName,
                FileUrl = url,
                UploadedAt = DateTime.UtcNow,
                UploadedByUserId = actorUserId
            });

            await _db.SaveChangesAsync(ct);

            await CreateVersionSnapshotInternalAsync(id, actorUserId, $"Add attachment: {fileName}", ct);

            await _audit.LogAsync(
                actorUserId,
                action: "UploadAttachment",
                entityType: "Contract",
                entityId: id.ToString(),
                note: fileName,
                oldValue: null,
                newValue: new { FileName = fileName, Url = url },
                ct: ct);

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // =========================
    // 7) VERSIONING
    // =========================
    public Task CreateVersionSnapshotAsync(int contractId, string changedByUserId, CancellationToken ct = default)
        => CreateVersionSnapshotInternalAsync((long)contractId, TryParseUserId(changedByUserId), "Manual snapshot", ct);

    public async Task<List<ContractVersionItemDto>> GetVersionsAsync(int contractId, CancellationToken ct = default)
    {
        var id = (long)contractId;

        var exists = await _db.Contracts.AsNoTracking().AnyAsync(x => x.ContractId == id, ct);
        if (!exists) throw new InvalidOperationException("Contract not found.");

        return await _db.ContractVersions.AsNoTracking()
            .Where(v => v.ContractId == id)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new ContractVersionItemDto
            {
                VersionId = v.VersionId,
                VersionNumber = v.VersionNumber,
                ChangedAt = v.ChangedAt,
                ChangedByUserId = v.ChangedByUserId,
                ChangeNote = v.ChangeNote,
                SnapshotJson = v.SnapshotJson
            })
            .ToListAsync(ct);
    }

    private async Task CreateVersionSnapshotInternalAsync(long contractId, int? changedByUserId, string? changeNote, CancellationToken ct)
    {
        var contract = await _db.Contracts.AsNoTracking()
            .Include(c => c.Attachments)
            .Include(c => c.Deposits)
            .FirstOrDefaultAsync(c => c.ContractId == contractId, ct);

        if (contract == null) return;

        var currentMaxVersion = await _db.ContractVersions
            .Where(v => v.ContractId == contractId)
            .MaxAsync(v => (int?)v.VersionNumber, ct) ?? 0;

        var snapshotObj = new
        {
            contract.ContractId,
            contract.ContractCode,
            contract.RoomId,
            contract.TenantId,
            contract.StartDate,
            contract.EndDate,
            contract.BaseRent,
            contract.DepositAmount,
            contract.DepositStatus,
            contract.DepositPaidAt,
            contract.DepositPaidAmount,
            contract.PaymentCycle,
            contract.Status,
            contract.Note,
            Attachments = contract.Attachments.Select(a => new
            {
                a.AttachmentId,
                a.FileName,
                a.FileUrl,
                a.UploadedAt,
                a.UploadedByUserId
            }),
            Deposits = contract.Deposits.Select(d => new
            {
                d.DepositId,
                d.Amount,
                d.Type,
                d.Note,
                d.CreatedAt
            })
        };

        var json = JsonSerializer.Serialize(snapshotObj, new JsonSerializerOptions { WriteIndented = true });

        _db.ContractVersions.Add(new ContractVersion
        {
            ContractId = contractId,
            VersionNumber = currentMaxVersion + 1,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId,
            ChangeNote = changeNote,
            SnapshotJson = json
        });

        await _db.SaveChangesAsync(ct);
    }

    // =========================
    // 8) REMINDERS (manual simple)
    // =========================
    public async Task CreateReminderAsync(int contractId, DateTime remindAt, string type, CancellationToken ct = default)
    {
        var id = (long)contractId;

        var contract = await _db.Contracts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ContractId == id, ct);

        if (contract == null) throw new InvalidOperationException("Contract not found.");

        var notification = new Notification
        {
            CreatedByUserId = contract.CreatedByUserId ?? 1, // demo fallback
            Title = $"Contract reminder ({type})",
            Message = $"Contract {contract.ContractCode} reminder at {remindAt:yyyy-MM-dd HH:mm}.",
            Type = "Reminder",
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);

        _db.NotificationRecipients.Add(new NotificationRecipient
        {
            NotificationId = notification.NotificationId,
            TenantId = contract.TenantId,
            IsRead = false
        });

        await _db.SaveChangesAsync(ct);
    }

    // =========================
    // 9) EXPORT PDF
    // =========================
    public async Task<byte[]> ExportPdfStubAsync(int contractId, int? actorUserId = null, CancellationToken ct = default)
    {
        var id = (long)contractId;

        var c = await _db.Contracts.AsNoTracking()
            .Include(x => x.Room)
            .Include(x => x.Tenant)
            .FirstOrDefaultAsync(x => x.ContractId == id, ct);

        if (c == null) throw new InvalidOperationException("Contract not found.");

        var bytes = BLL.Pdf.ContractPdfExporter.Export(c);

        await _audit.LogAsync(
            actorUserId,
            action: "ExportContractPdf",
            entityType: "Contract",
            entityId: c.ContractId.ToString(),
            note: c.ContractCode,
            oldValue: null,
            newValue: new { c.ContractId, c.ContractCode, SizeBytes = bytes.Length },
            ct: ct);

        return bytes;
    }

    // =========================
    // 10) EXPIRY JOB (demo scan)
    // =========================
    public async Task<int> ScanAndSendExpiryRemindersAsync(
        int daysBeforeEnd = 7,
        string remindType = "Expiry_7d",
        int? actorUserId = null,
        CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var target = today.AddDays(daysBeforeEnd);

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        try
        {
            var contracts = await _db.Contracts.AsNoTracking()
                .Where(c => c.Status == ContractStatus.Active && c.EndDate.Date == target.Date)
                .Select(c => new { c.ContractId, c.ContractCode, c.TenantId })
                .ToListAsync(ct);

            var sent = 0;

            foreach (var c in contracts)
            {
                var already = await _db.ContractReminderLogs.AsNoTracking()
                    .AnyAsync(x =>
                        x.ContractId == c.ContractId &&
                        x.RemindType == remindType &&
                        x.RemindAtDate == today, ct);

                if (already) continue;

                var notif = new Notification
                {
                    CreatedByUserId = (int)actorUserId,
                    Title = $"Contract Expiry Reminder ({daysBeforeEnd} days)",
                    Message = $"Contract {c.ContractCode} will expire on {target:yyyy-MM-dd}. Please review / renew / terminate.",
                    Type = "Reminder",
                    CreatedAt = DateTime.UtcNow
                };

                _db.Notifications.Add(notif);
                await _db.SaveChangesAsync(ct); // get notif id

                _db.NotificationRecipients.Add(new NotificationRecipient
                {
                    NotificationId = notif.NotificationId,
                    TenantId = c.TenantId,
                    IsRead = false
                });

                _db.ContractReminderLogs.Add(new ContractReminderLog
                {
                    ContractId = c.ContractId,
                    RemindType = remindType,
                    RemindAtDate = today,
                    CreatedAt = DateTime.UtcNow
                });

                sent++;
            }

            if (sent > 0)
                await _db.SaveChangesAsync(ct);

            await _audit.LogAsync(
                actorUserId,
                action: "RunExpiryReminderJob",
                entityType: "Contract",
                entityId: "0",
                note: "Scan expiry reminders",
                oldValue: null,
                newValue: new
                {
                    daysBeforeEnd,
                    remindType,
                    targetEndDate = target.ToString("yyyy-MM-dd"),
                    sent
                },
                ct: ct);

            await tx.CommitAsync(ct);
            return sent;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // =========================
    // Helpers: validation + mapping
    // =========================
    private static void ValidateDateRange(DateTime start, DateTime end, string startName, string endName)
    {
        if (start.Date >= end.Date)
            throw new InvalidOperationException($"{startName} must be earlier than {endName}.");
    }

    private static void EnsureActive(Contract c, string actionName)
    {
        if (!ContractStatus.Is(c.Status, ContractStatus.Active))
            throw new InvalidOperationException(
                $"Only ACTIVE contracts can be {actionName}. Current status = '{c.Status}'.");
    }

    private static ContractDto MapToDto(Contract c) => new()
    {
        Id = (int)c.ContractId,
        RoomId = c.RoomId,
        TenantId = c.TenantId,
        StartDate = c.StartDate,
        EndDate = c.EndDate,
        Rent = c.BaseRent,
        Deposit = c.DepositAmount,
        Status = StatusToInt(c.Status),
        IsActive = c.Status == ContractStatus.Active,

        ContractCode = c.ContractCode,
        DepositStatus = c.DepositStatus ?? "Unpaid",
        DepositPaidAt = c.DepositPaidAt,
        DepositPaidAmount = c.DepositPaidAmount
    };

    private static int StatusToInt(string? status) => status switch
    {
        ContractStatus.Pending => 0,
        ContractStatus.Active => 1,
        ContractStatus.Expired => 2,
        ContractStatus.Terminated => 3,
        ContractStatus.Renewed => 4,
        _ => 99
    };

    private static string AppendNote(string? old, string add)
        => string.IsNullOrWhiteSpace(old) ? add : $"{old}\n- {add}";

    private async Task<string> GenerateContractCodeAsync(CancellationToken ct)
    {
        var prefix = $"CT-{DateTime.UtcNow:yyyyMM}-";
        var last = await _db.Contracts.AsNoTracking()
            .Where(c => c.ContractCode.StartsWith(prefix))
            .OrderByDescending(c => c.ContractCode)
            .Select(c => c.ContractCode)
            .FirstOrDefaultAsync(ct);

        var nextNum = 1;
        if (!string.IsNullOrWhiteSpace(last))
        {
            var tail = last.Replace(prefix, "");
            if (int.TryParse(tail, out var n)) nextNum = n + 1;
        }

        return $"{prefix}{nextNum:0000}";
    }

    private static int? TryParseUserId(string? s)
        => int.TryParse(s, out var id) ? id : null;
}