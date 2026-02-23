using BLL.DTOs.Contract;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebHostRazor.Pages.Host.Contracts;

public class RenewModel : PageModel
{
    private readonly IContractService _service;
    public RenewModel(IContractService service) => _service = service;

    public ContractDto? Current { get; set; }

    [BindProperty] public DateTime NewStartDate { get; set; }
    [BindProperty] public DateTime NewEndDate { get; set; }
    [BindProperty] public decimal NewRent { get; set; }
    [BindProperty] public decimal NewDeposit { get; set; }
    [BindProperty] public string? Reason { get; set; }

    public async Task<IActionResult> OnGet(long id)
    {
        Current = await _service.GetByIdAsync((int)id);

        if (!Current.IsActive)
        {
            TempData["Err"] = $"Contract is not Active (Status={Current.Status}).";
            return RedirectToPage("./Details", new { id });
        }

        // policy: next day after old end
        NewStartDate = Current.EndDate.Date.AddDays(1);
        NewEndDate = NewStartDate.AddMonths(6);
        NewRent = Current.Rent;
        NewDeposit = Current.Deposit;
        return Page();
    }

    public async Task<IActionResult> OnPost(long id)
    {
        try
        {
            var created = await _service.RenewAsync(new RenewContractDto
            {
                ContractId = (int)id,
                NewStartDate = NewStartDate,
                NewEndDate = NewEndDate,
                NewRent = NewRent,
                NewDeposit = NewDeposit,
                Reason = Reason,
                RequireStartNextDay = true
            });

            return RedirectToPage("./Details", new { id = created.Id });
        }
        catch (Exception ex)
        {
            TempData["Err"] = ex.Message;
            return RedirectToPage("./Details", new { id });
        }
    }
}
