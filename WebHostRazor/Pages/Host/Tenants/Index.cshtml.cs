using BLL.Common;
using BLL.DTOs.Tenant;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebHostRazor.Pages.Host.Tenants;

public class IndexModel : PageModel
{
    private readonly ITenantService _tenantService;

    public IndexModel(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public List<TenantDto> Tenants { get; set; } = new();

    public async Task OnGetAsync()
    {
        var result = await _tenantService.GetTenantsAsync(
            new BLL.DTOs.Common.PagedRequestDto
            {
                PageNumber = 1,
                PageSize = 100
            });

        Tenants = result.Items.ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _tenantService.DeleteAsync(id);
            return RedirectToPage("Index");
        }
        catch (Exceptions.NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}