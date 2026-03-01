using BLL.DTOs.Billing;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Entities.Billing;
using DAL.Entities.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class BillingService : IBillingService
{
    private readonly AppDbContext _db;
    private readonly IUtilityService _utility;

    public BillingService(AppDbContext db, IUtilityService utility)
    {
        _db = db;
        _utility = utility;
    }

    // =========================
    // LIST
    // =========================
    public async Task<List<BillDto>> GetBillsAsync(string? q, string? status, string? month, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // ✅ 0) Refresh Overdue (Issued + quá hạn => Overdue)
        await _db.Bills
            .Where(b => b.Status == BillStatus.Issued && b.DueDate < now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(b => b.Status, BillStatus.Overdue)
                .SetProperty(b => b.UpdatedAt, now),
                ct);

        var query = _db.Bills
            .AsNoTracking()
            .Include(b => b.Contract).ThenInclude(c => c.Room)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim();
            query = query.Where(b =>
                b.Contract.Room.RoomNo.Contains(kw) ||
                (b.Contract.Room.Name != null && b.Contract.Room.Name.Contains(kw)));
        }

        if (!string.IsNullOrWhiteSpace(month) && TryParseMonth(month, out var period))
            query = query.Where(b => b.Period == period);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = NormalizeUiStatus(status);

            // ✅ 1 nguồn sự thật: filter theo Status
            query = s switch
            {
                "Paid" => query.Where(b => b.Status == BillStatus.Paid),
                "Overdue" => query.Where(b => b.Status == BillStatus.Overdue),
                _ => query.Where(b => b.Status == BillStatus.Issued) // Unpaid
            };
        }

        return await query
            .OrderByDescending(b => b.Period)
            .ThenBy(b => b.Contract.Room.RoomNo)
            .Select(b => new BillDto
            {
                Id = b.Id,
                ContractId = b.ContractId,
                Period = b.Period,
                IssuedAt = b.IssuedAt,
                DueDate = b.DueDate,
                Status = (int)b.Status,
                TotalAmount = b.TotalAmount,
                RoomName = b.Contract.Room.Name ?? b.Contract.Room.RoomNo,
                Items = new List<BillItemDto>(),
                Payments = new List<PaymentDto>()
            })
            .ToListAsync(ct);
    }

    // =========================
    // DETAILS
    // =========================
    public async Task<BillDto?> GetBillAsync(int id, CancellationToken ct)
    {
        var b = await _db.Bills
            .AsNoTracking()
            .Include(x => x.Contract).ThenInclude(c => c.Room)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (b == null) return null;

        var items = await _db.BillItems
            .AsNoTracking()
            .Where(i => i.BillId == id)
            .OrderBy(i => i.ExtraFeeId.HasValue)
            .ThenBy(i => i.Name)
            .Select(i => new BillItemDto
            {
                Id = i.Id,
                BillId = i.BillId,
                Name = i.Name,
                Amount = i.Amount,
                ExtraFeeId = i.ExtraFeeId
            })
            .ToListAsync(ct);

        var payments = await _db.Payments
            .AsNoTracking()
            .Where(p => p.BillId == id)
            .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                BillId = p.BillId,
                Amount = p.Amount,
                Method = p.Method,
                Status = (int)p.Status,
                PaidAt = p.PaidAt,
                TransactionRef = p.TransactionRef
            })
            .ToListAsync(ct);

        return new BillDto
        {
            Id = b.Id,
            ContractId = b.ContractId,
            Period = b.Period,
            IssuedAt = b.IssuedAt,
            DueDate = b.DueDate,
            Status = (int)b.Status,
            TotalAmount = b.TotalAmount,
            RoomName = b.Contract.Room.Name ?? b.Contract.Room.RoomNo,
            Items = items,
            Payments = payments
        };
    }

    // =========================
    // DROPDOWN Rooms
    // =========================
    public async Task<List<SelectListItem>> GetActiveRoomOptionsAsync(CancellationToken ct)
    {
        var rooms = await _db.Contracts
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Include(c => c.Room)
            .Select(c => c.Room)
            .Distinct()
            .OrderBy(r => r.RoomNo)
            .ToListAsync(ct);

        return rooms.Select(r => new SelectListItem
        {
            Value = r.RoomNo,
            Text = $"{r.RoomNo} - {(r.Name ?? "Room")}"
        }).ToList();
    }

    // =========================
    // ExtraFee
    // =========================
    public async Task<List<ExtraFee>> GetActiveExtraFeesAsync(CancellationToken ct)
    {
        return await _db.ExtraFees
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    // =========================
    // CREATE
    // =========================
    public async Task<(bool Ok, string? Error)> CreateBillAsync(string roomNo, string month, string uiStatus, decimal total, CancellationToken ct)
    {
        if (!TryParseMonth(month, out var period))
            return (false, "Month phải theo định dạng yyyy-MM (vd: 2026-02).");

        if (string.IsNullOrWhiteSpace(roomNo))
            return (false, "Vui lòng chọn phòng.");

        if (!IsUiStatusValid(uiStatus))
            return (false, "Status chỉ nhận: Unpaid, Paid, Overdue.");

        var roomKey = roomNo.Trim();

        var contract = await _db.Contracts
            .Include(c => c.Room)
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(c =>
                c.Room.RoomNo == roomKey ||
                (c.Room.Name != null && c.Room.Name == roomKey), ct);

        if (contract == null)
            return (false, "Không tìm thấy phòng hoặc chưa có hợp đồng active cho phòng này.");

        var exists = await _db.Bills.AnyAsync(b => b.ContractId == contract.Id && b.Period == period, ct);
        if (exists)
            return (false, "Bill của phòng này trong tháng này đã tồn tại.");

        var now = DateTime.UtcNow;

        var bill = new Bill
        {
            ContractId = contract.Id,
            Period = period,
            IssuedAt = now,
            DueDate = now.AddDays(7),
            Status = FromUiStatus(uiStatus),
            TotalAmount = total,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Bills.Add(bill);
        await _db.SaveChangesAsync(ct);

        return (true, null);
    }

    // =========================
    // UPDATE
    // =========================
    public async Task<(bool Ok, string? Error)> UpdateBillAsync(int id, string month, string uiStatus, decimal total, CancellationToken ct)
    {
        if (!TryParseMonth(month, out var period))
            return (false, "Month phải theo định dạng yyyy-MM (vd: 2026-02).");

        if (!IsUiStatusValid(uiStatus))
            return (false, "Status chỉ nhận: Unpaid, Paid, Overdue.");

        var bill = await _db.Bills.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (bill == null) return (false, "Bill không tồn tại.");

        bill.Period = period;
        bill.TotalAmount = total;
        bill.Status = FromUiStatus(uiStatus);
        bill.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return (true, null);
    }

    // =========================
    // DELETE (safe FK)
    // =========================
    public async Task<(bool Ok, string? Error)> DeleteBillAsync(int id, CancellationToken ct)
    {
        var bill = await _db.Bills.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (bill == null) return (false, "Bill không tồn tại.");

        // safe: xóa con trước để khỏi lỗi FK
        _db.BillItems.RemoveRange(_db.BillItems.Where(x => x.BillId == id));
        _db.Payments.RemoveRange(_db.Payments.Where(x => x.BillId == id));

        _db.Bills.Remove(bill);
        await _db.SaveChangesAsync(ct);

        return (true, null);
    }

    // =========================
    // PAYMENT
    // =========================
    public async Task<(bool Ok, string? Error)> RecordPaymentAsync(RecordPaymentDto dto, CancellationToken ct)
    {
        var bill = await _db.Bills.FirstOrDefaultAsync(b => b.Id == dto.BillId, ct);
        if (bill == null) return (false, "Bill không tồn tại.");

        if (dto.Amount <= 0) return (false, "Số tiền phải lớn hơn 0.");

        var now = DateTime.UtcNow;

        var payment = new Payment
        {
            BillId = dto.BillId,
            Amount = dto.Amount,
            Method = dto.Method,
            Status = PaymentStatus.Completed,
            PaidAt = dto.PaidAt ?? now,
            TransactionRef = dto.TransactionRef,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        var totalPaid = await _db.Payments
       .Where(p => p.BillId == dto.BillId && p.Status == PaymentStatus.Completed)
       .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

        // ✅ Update BillStatus theo nghiệp vụ chuẩn
        if (totalPaid >= bill.TotalAmount)
        {
            bill.Status = BillStatus.Paid;
        }
        else if (bill.DueDate < now)
        {
            bill.Status = BillStatus.Overdue;
        }
        else
        {
            bill.Status = BillStatus.Issued;
        }

        bill.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        return (true, null);
    }

    // =========================
    // GENERATE BILLS
    // =========================
    public async Task<(bool Ok, string? Error, int Created, int Skipped)> GenerateBillsAsync(
        GenerateBillsRequestDto req,
        List<int> extraFeeIds,
        bool includeRent,
        bool includeUtilities,
        CancellationToken ct)
    {
        var period = req.Period;
        if (period <= 0) return (false, "Period không hợp lệ (YYYYMM).", 0, 0);

        var y = period / 100;
        var m = period % 100;
        if (m < 1 || m > 12) return (false, "Period không hợp lệ (YYYYMM).", 0, 0);

        if (req.DueDate == default) return (false, "Vui lòng chọn Due Date.", 0, 0);

        var firstDay = new DateTime(y, m, 1);
        var lastDay = new DateTime(y, m, DateTime.DaysInMonth(y, m));
        if (req.DueDate < firstDay || req.DueDate > lastDay)
            return (false, $"Due Date phải nằm trong tháng {y:D4}-{m:D2}.", 0, 0);

        // Lấy contracts active (project nhẹ để tránh phụ thuộc vào tên property Rent/RoomId)
        var contractRows = await _db.Contracts
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new
            {
                c.Id,
                RoomId = EF.Property<int>(c, "RoomId"),
                Rent = EF.Property<decimal>(c, "Rent")
            })
            .ToListAsync(ct);

        // Nếu bạn muốn filter block/floor:
        // -> cần Room entity có BlockId/FloorId. Mình để comment vì chưa thấy Room.cs của bạn.

        var existingContractIds = (await _db.Bills
                .AsNoTracking()
                .Where(b => b.Period == period)
                .Select(b => b.ContractId)
                .ToListAsync(ct))
            .ToHashSet();

        var fees = (extraFeeIds != null && extraFeeIds.Count > 0)
            ? await _db.ExtraFees.AsNoTracking()
                .Where(x => x.IsActive && extraFeeIds.Contains(x.Id))
                .ToListAsync(ct)
            : new List<ExtraFee>();

        int created = 0, skipped = 0;

        foreach (var c in contractRows)
        {
            if (existingContractIds.Contains(c.Id))
            {
                skipped++;
                continue;
            }

            var bill = new Bill
            {
                ContractId = c.Id,
                Period = period,
                DueDate = req.DueDate,
                Status = BillStatus.Issued,
                IssuedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Bills.Add(bill);
            await _db.SaveChangesAsync(ct); // lấy bill.Id

            if (includeRent)
            {
                _db.BillItems.Add(new BillItem
                {
                    BillId = bill.Id,
                    Name = "Rent",
                    Amount = c.Rent
                });
            }

            if (includeUtilities)
            {
                try
                {
                    var charges = await _utility.CalculateChargesAsync(c.RoomId, period, ct);

                    if (charges.ElectricAmount > 0)
                        _db.BillItems.Add(new BillItem { BillId = bill.Id, Name = "Electric", Amount = charges.ElectricAmount });

                    if (charges.WaterAmount > 0)
                        _db.BillItems.Add(new BillItem { BillId = bill.Id, Name = "Water", Amount = charges.WaterAmount });
                }
                catch
                {
                    // chưa có readings -> skip utilities
                }
            }

            foreach (var fee in fees)
            {
                _db.BillItems.Add(new BillItem
                {
                    BillId = bill.Id,
                    Name = fee.Name,
                    Amount = fee.DefaultAmount,
                    ExtraFeeId = fee.Id
                });
            }

            await _db.SaveChangesAsync(ct);

            bill.TotalAmount = await _db.BillItems
                .Where(x => x.BillId == bill.Id)
                .SumAsync(x => x.Amount, ct);

            bill.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            created++;
        }

        return (true, null, created, skipped);
    }

    // ================= Helpers =================
    private static bool TryParseMonth(string? month, out int period)
    {
        period = 0;
        if (string.IsNullOrWhiteSpace(month)) return false;

        if (!DateTime.TryParseExact(month.Trim(), "yyyy-MM",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dt))
            return false;

        period = dt.Year * 100 + dt.Month;
        return true;
    }

    private static bool IsUiStatusValid(string? ui)
    {
        var s = NormalizeUiStatus(ui);
        return s is "Unpaid" or "Paid" or "Overdue";
    }

    private static string NormalizeUiStatus(string? ui)
    {
        if (string.IsNullOrWhiteSpace(ui)) return "Unpaid";
        var s = ui.Trim();
        if (s.Equals("paid", StringComparison.OrdinalIgnoreCase)) return "Paid";
        if (s.Equals("overdue", StringComparison.OrdinalIgnoreCase)) return "Overdue";
        return "Unpaid";
    }

    private static BillStatus FromUiStatus(string ui) => NormalizeUiStatus(ui) switch
    {
        "Paid" => BillStatus.Paid,
        "Overdue" => BillStatus.Overdue,
        _ => BillStatus.Issued
    };
}