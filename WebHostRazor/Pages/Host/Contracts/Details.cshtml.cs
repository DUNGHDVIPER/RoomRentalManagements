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
}
