using System.Security.Claims;
using BLL.DTOs.Common;
using BLL.DTOs.Contract;
using BLL.Services.Interfaces;
using DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAdmin.MVC.Models.Contracts;

namespace WebAdmin.MVC.Controllers;

// tạm thời bỏ login
[AllowAnonymous]
// Nếu bật lại login sau này:
// [Authorize(Roles = "Admin,Host")]
[Route("Contracts")]
public class ContractsController : Controller
{
    private readonly MotelManagementDbContext _db;
    private readonly IContractService _contractService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        MotelManagementDbContext db,
        IContractService contractService,
        IWebHostEnvironment env,
        ILogger<ContractsController> logger)
    {
        _db = db;
        _contractService = contractService;
        _env = env;
        _logger = logger;
    }

    private int? GetActorUserIdInt()
    {
        var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(s, out var id) ? id : null;
    }

    // =========================
    // INDEX
    // GET /Contracts  or /Contracts/Index
    // =========================
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string? q, int? status, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var req = new PagedRequestDto(page, pageSize)
        {
            Keyword = q
        };

        var result = await _contractService.GetContractsAsync(req, ct);

        // filter status ở MVC (service hiện tại chưa filter)
        var items = result.Items.AsEnumerable();
        if (status.HasValue)
            items = items.Where(x => x.Status == status.Value);

        var list = items.Select(c => new ContractRowVm
        {
            ContractId = c.Id,
            ContractCode = c.ContractCode,
            RoomId = c.RoomId,
            TenantId = c.TenantId,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Rent = c.Rent,
            Deposit = c.Deposit,
            Status = c.Status,
            IsActive = c.IsActive
        }).ToList();

        var vm = new ContractsIndexVm
        {
            Query = q,
            Status = status,
            Page = result.PageNumber,
            PageSize = result.PageSize,
            Total = status.HasValue ? list.Count : result.TotalCount,
            Items = list
        };

        return View("Index", vm);
    }
    // =========================
    // CREATE
    // GET /Contracts/Create
    // POST /Contracts/Create
    // =========================
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("Create", new ContractCreateVm());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractCreateVm vm, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return View("Create", vm);

        try
        {
            var actorUserId = GetActorUserIdInt();

            var dto = new CreateContractDto
            {
                RoomId = vm.RoomId,
                TenantId = vm.TenantId,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                Rent = vm.Rent,
                Deposit = vm.Deposit,
                ActivateNow = vm.ActivateNow
            };

            var created = await _contractService.CreateAsync(dto, actorUserId, ct);
            TempData["Success"] = "Created successfully.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("Create", vm);
        }
    }


[HttpPost("Activate/{id:int}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Activate(int id, CancellationToken ct = default)
{
    try
    {
        var contract = await _db.Contracts.FirstOrDefaultAsync(x => x.ContractId == (long)id, ct);
        if (contract == null)
        {
            TempData["Error"] = "Contract not found.";
            return RedirectToAction(nameof(Index));
        }

        // Nếu đã Active thì thôi
        if (string.Equals(contract.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Success"] = "This contract is already ACTIVE.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ✅ Check trước: room đã có ACTIVE contract chưa?
        var hasActive = await _db.Contracts.AsNoTracking()
            .AnyAsync(c => c.RoomId == contract.RoomId
                        && c.Status == "Active"
                        && c.ContractId != contract.ContractId, ct);

        if (hasActive)
        {
            TempData["Error"] =
                $"Cannot activate this contract because Room {contract.RoomId} already has an ACTIVE contract. " +
                $"Please terminate/renew the current ACTIVE contract first (only one ACTIVE contract per room).";
            return RedirectToAction(nameof(Details), new { id });
        }

        contract.Status = "Active";
        await _db.SaveChangesAsync(ct);

        TempData["Success"] = "Activated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }
    catch (DbUpdateException)
    {
        // ✅ fallback nếu vẫn bị constraint (race condition)
        TempData["Error"] =
            "Cannot activate this contract because the room already has an ACTIVE contract (one active contract per room). " +
            "Please terminate/renew the current ACTIVE contract first.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
// =========================
// DETAILS
// GET /Contracts/Details/4
// =========================
[HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        try
        {
            var dto = await _contractService.GetByIdAsync(id, ct);

            var attachments = await _db.ContractAttachments.AsNoTracking()
                .Where(a => a.ContractId == (long)id)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new AttachmentRowVm
                {
                    FileName = a.FileName,
                    Url = a.FileUrl,
                    ContentType = null,
                    Size = 0,
                    UploadedAt = a.UploadedAt
                })
                .ToListAsync(ct);

            var vm = new ContractDetailsVm
            {
                Contract = dto,
                Attachments = attachments
            };

            return View("Details", vm);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // =========================
    // RENEW
    // GET /Contracts/Renew/4
    // POST /Contracts/Renew/4
    // =========================
    [HttpGet("Renew/{id:int}")]
    public async Task<IActionResult> Renew(int id, CancellationToken ct = default)
    {
        try
        {
            var c = await _contractService.GetByIdAsync(id, ct);
            /*if (!c.IsActive)
            {
                TempData["Error"] = "Không thể Renew vì hợp đồng chưa ACTIVE. Hãy bấm Activate trước.";
                return RedirectToAction(nameof(Details), new { id });
            }*/
            if (c.Status == 0) // Pending
            {
                TempData["Error"] = "Contract is PENDING. Please Activate before renewing.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (c.Status == 4) // Renewed
            {
                TempData["Error"] = "Contract already renewed.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var vm = new RenewVm
            {
                ContractId = c.Id,
                CurrentStatus = c.Status,
                CurrentStartDate = c.StartDate,
                CurrentEndDate = c.EndDate,
                CurrentRent = c.Rent,
                CurrentDeposit = c.Deposit,

                NewStartDate = c.EndDate.AddDays(1),
                NewEndDate = c.EndDate.AddMonths(12),
                NewRent = c.Rent,
                NewDeposit = c.Deposit
            };

            return View("Renew", vm);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost("Renew/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Renew(int id, RenewVm vm, CancellationToken ct = default)
    {
        vm.ContractId = id;

        if (vm.NewEndDate < vm.NewStartDate)
            ModelState.AddModelError(nameof(vm.NewEndDate), "New end date must be after start date.");

        if (!ModelState.IsValid)
        {
            await TryReloadRenewHeader(vm, ct);
            return View("Renew", vm);
        }

        try
        {
            var actorUserId = GetActorUserIdInt();

            var dto = new RenewContractDto
            {
                ContractId = vm.ContractId,
                NewStartDate = vm.NewStartDate,
                NewEndDate = vm.NewEndDate,
                NewRent = vm.NewRent,
                NewDeposit = vm.NewDeposit,
                Reason = vm.Reason,
                RequireStartNextDay = true
            };

            var renewed = await _contractService.RenewAsync(dto, actorUserId, ct);

            TempData["Success"] = "Renewed successfully.";
            return RedirectToAction(nameof(Details), new { id = renewed.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = vm.ContractId });
        }
    }

    private async Task TryReloadRenewHeader(RenewVm vm, CancellationToken ct)
    {
        try
        {
            var c = await _contractService.GetByIdAsync(vm.ContractId, ct);
            vm.CurrentStatus = c.Status;
            vm.CurrentStartDate = c.StartDate;
            vm.CurrentEndDate = c.EndDate;
            vm.CurrentRent = c.Rent;
            vm.CurrentDeposit = c.Deposit;
        }
        catch { }
    }

    // =========================
    // TERMINATE
    // GET /Contracts/Terminate/4
    // POST /Contracts/Terminate/4
    // =========================
    [HttpGet("Terminate/{id:int}")]
    public async Task<IActionResult> Terminate(int id, CancellationToken ct = default)
    {
        try
        {
            var c = await _contractService.GetByIdAsync(id, ct);

            if (!c.IsActive) // hoặc c.Status != 1
            {
                TempData["Error"] = "Contract is not ACTIVE. Please activate this contract before terminating.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var today = DateTime.Today;
            var terminateDate = today < c.StartDate ? c.StartDate
                              : today > c.EndDate ? c.EndDate
                              : today;

            var vm = new TerminateVm
            {
                ContractId = c.Id,
                TerminateDate = terminateDate,
                CurrentStatus = c.Status,
                CurrentStartDate = c.StartDate,
                CurrentEndDate = c.EndDate,
                CurrentRent = c.Rent,
                CurrentDeposit = c.Deposit
            };

            return View("Terminate", vm);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost("Terminate/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Terminate(int id, TerminateVm vm, CancellationToken ct = default)
    {
        vm.ContractId = id;

        if (!ModelState.IsValid)
        {
            await TryReloadTerminateHeader(vm, ct);
            return View("Terminate", vm);
        }

        try
        {
            var actorUserId = GetActorUserIdInt();

            var dto = new TerminateContractDto
            {
                ContractId = vm.ContractId,
                TerminateDate = vm.TerminateDate,
                Reason = vm.Reason
            };

            await _contractService.TerminateAsync(dto, actorUserId, ct);

            TempData["Success"] = "Terminated successfully.";
            return RedirectToAction(nameof(Details), new { id = vm.ContractId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await TryReloadTerminateHeader(vm, ct);
            return View("Terminate", vm);
        }
    }

    private async Task TryReloadTerminateHeader(TerminateVm vm, CancellationToken ct)
    {
        try
        {
            var c = await _contractService.GetByIdAsync(vm.ContractId, ct);
            vm.CurrentStatus = c.Status;
            vm.CurrentStartDate = c.StartDate;
            vm.CurrentEndDate = c.EndDate;
            vm.CurrentRent = c.Rent;
            vm.CurrentDeposit = c.Deposit;
        }
        catch { }
    }

    // =========================
    // UPLOAD ATTACHMENT
    // GET /Contracts/UploadAttachment/4
    // POST /Contracts/UploadAttachment/4
    // =========================
    [HttpGet("UploadAttachment/{id:int}")]
    public async Task<IActionResult> UploadAttachment(int id, CancellationToken ct = default)
    {
        var vm = new UploadAttachmentVm { ContractId = id };

        vm.Attachments = await _db.ContractAttachments.AsNoTracking()
            .Where(a => a.ContractId == (long)id)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new AttachmentRowVm
            {
                FileName = a.FileName,
                Url = a.FileUrl,
                ContentType = null,
                Size = 0,
                UploadedAt = a.UploadedAt
            })
            .ToListAsync(ct);

        return View("UploadAttachment", vm);
    }

    [HttpPost("UploadAttachment/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile? file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            ModelState.AddModelError("", "Please choose a file.");
        else
            ValidateAttachment(file);

        if (!ModelState.IsValid)
            return await UploadAttachment(id, ct);

        try
        {
            var actorUserId = GetActorUserIdInt();
            var saved = await SaveContractAttachmentAsync(id, file!, ct);

            await _contractService.AddAttachmentStubAsync(
                contractId: id,
                fileName: saved.StoredFileName,
                url: saved.PublicUrl,
                actorUserId: actorUserId,
                ct: ct);

            TempData["Success"] = "Uploaded successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return await UploadAttachment(id, ct);
        }
    }

    private void ValidateAttachment(IFormFile file)
    {
        const long maxBytes = 5 * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            ModelState.AddModelError("", "File too large. Max 5MB.");
            return;
        }

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        var allowedExt = new HashSet<string> { ".pdf", ".jpg", ".jpeg", ".png" };
        if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext))
        {
            ModelState.AddModelError("", "Only PDF/JPG/PNG are allowed.");
            return;
        }

        var contentType = (file.ContentType ?? "").ToLowerInvariant();
        var okCt = contentType == "application/pdf" || contentType == "image/jpeg" || contentType == "image/png";
        if (!okCt)
            ModelState.AddModelError("", "Invalid content type. Only PDF/JPG/PNG are allowed.");
    }

    private async Task<SavedAttachment> SaveContractAttachmentAsync(int contractId, IFormFile file, CancellationToken ct)
    {
        // shared folder: ../SharedUploads/contracts
        var sharedContractsDir = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "SharedUploads", "contracts"));
        Directory.CreateDirectory(sharedContractsDir);

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        var uniqueName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";

        var path = Path.Combine(sharedContractsDir, uniqueName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream, ct);

        return new SavedAttachment
        {
            StoredFileName = uniqueName,
            PublicUrl = $"/uploads/contracts/{uniqueName}" // ✅ URL chung
        };
    }

    private sealed class SavedAttachment
    {
        public string StoredFileName { get; init; } = "";
        public string PublicUrl { get; init; } = "";
    }

    // =========================
    // EXPORT PDF
    // GET /Contracts/ExportPdf/4
    // =========================
    [HttpGet("ExportPdf/{id:int}")]
    public async Task<IActionResult> ExportPdf(int id, CancellationToken ct = default)
    {
        try
        {
            var actorUserId = GetActorUserIdInt();
            var bytes = await _contractService.ExportPdfStubAsync(id, actorUserId, ct);
            return File(bytes, "application/pdf", $"Contract-{id}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // =========================
    // VERSIONS
    // GET /Contracts/Versions/4
    // =========================
    [HttpGet("Versions/{id:int}")]
    public async Task<IActionResult> Versions(int id, CancellationToken ct = default)
    {
        try
        {
            var versions = await _contractService.GetVersionsAsync(id, ct);

            var vm = new ContractVersionsVm
            {
                ContractId = id,
                Versions = versions.Select(v => new ContractVersionVm
                {
                    VersionId = v.VersionId,
                    VersionNumber = v.VersionNumber,
                    ChangedAt = v.ChangedAt,
                    ChangedByUserId = v.ChangedByUserId,
                    ChangeNote = v.ChangeNote,
                    SnapshotJson = v.SnapshotJson
                }).ToList()
            };

            return View("Versions", vm);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}