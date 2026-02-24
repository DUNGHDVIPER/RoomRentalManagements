using BLL.DTOs.Contract;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebHostRazor.Pages.Host.Contracts;

public class DetailsModel : PageModel
{
    private readonly IContractService _service;
    public DetailsModel(IContractService service) => _service = service;

    public ContractDto? Contract { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        try
        {
            Contract = await _service.GetByIdAsync(id);
            return Page();
        }
        catch (Exception ex)
        {
            TempData["Err"] = ex.Message;
            return RedirectToPage("/Host/Contracts/Index");
        }
    }
    public async Task<IActionResult> OnPostActivateAsync(int id, CancellationToken ct)
    {
        try
        {
            // activate trực tiếp DB thì cần DbContext; nếu HostRazor không có DbContext thì bạn làm Activate bên MVC thôi.
            TempData["Error"] = "Activate handler not implemented in HostRazor.";
            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage(new { id });
        }
    }
}
