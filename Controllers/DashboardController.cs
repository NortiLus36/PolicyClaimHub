using Microsoft.AspNetCore.Mvc;

namespace PolicyClaimHub.Controllers;

public sealed class DashboardController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
