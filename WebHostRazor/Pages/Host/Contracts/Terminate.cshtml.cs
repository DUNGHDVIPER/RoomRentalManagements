using BLL.DTOs.Contract;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebHostRazor.Security;

namespace WebHostRazor.Pages.Host.Contracts;

public class TerminateModel : PageModel
{
    private readonly IContractService _service;
    public TerminateModel(IContractService service) => _service = service;

    public ContractDto? Current { get; set; }

    [BindProperty] public DateTime TerminateDate { get; set; }
    [BindProperty] public string? Reason { get; set; }
   

    public async Task<IActionResult> OnGet(long id)
    {
        Current = await _service.GetByIdAsync((int)id);

        // default terminate date = today but clamp to [Start, End]
        var today = DateTime.Today;
        if (today < Current.StartDate.Date) today = Current.StartDate.Date;
        if (today > Current.EndDate.Date) today = Current.EndDate.Date;

        TerminateDate = today;
        Reason = "Terminate";
        return Page();
    }

    public async Task<IActionResult> OnPost(long id)
    {
        try
        {
            var actorUserId = User.GetActorUserId();   // ✅ ĐẶT Ở ĐÂY (WEB)
            var dto = new TerminateContractDto
            {
                ContractId = (int)id,
                TerminateDate = TerminateDate,
                Reason = Reason
            };

            await _service.TerminateAsync(dto, actorUserId);

            TempData["Ok"] = "Terminate success.";
            return RedirectToPage("./Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Err"] = ex.Message;
            return RedirectToPage("./Details", new { id });
        }
    }
}
