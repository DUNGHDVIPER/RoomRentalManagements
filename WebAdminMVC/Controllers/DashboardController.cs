using Microsoft.AspNetCore.Mvc;

using WebAdmin.MVC.Models.Dashboard;

namespace WebAdmin.MVC.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        return View(new DashboardVm());
    }
}
