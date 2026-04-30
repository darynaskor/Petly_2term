using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Petly.Business.Services;

namespace Petly.Controllers;

[Authorize(Roles = "system_admin")]
public class AdminController : Controller
{
    private readonly DashboardService _dashboardService;

    public AdminController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Analytics(int days = 30, int? shelterId = null)
    {
        var model = await _dashboardService.GetAnalyticsAsync(days, shelterId);
        return View(model);
    }
}
