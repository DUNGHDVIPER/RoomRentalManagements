using BLL.DTOs.Contract;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebHostRazor.Pages.Host.Contracts;

public class VersionsModel : PageModel
{
    private readonly IContractService _service;
    public VersionsModel(IContractService service) => _service = service;

    public int ContractId { get; set; }
    public List<ContractVersionItemDto> Items { get; set; } = new();

    public async Task<IActionResult> OnGet(int id)
    {
        try
        {
            ContractId = id;
            Items = await _service.GetVersionsAsync(id);
            return Page();
        }
        catch (Exception ex)
        {
            TempData["Err"] = ex.Message;
            return RedirectToPage("./Details", new { id });
        }
    }
}