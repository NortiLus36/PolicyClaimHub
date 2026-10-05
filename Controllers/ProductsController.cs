using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolicyClaimHub.Data;

namespace PolicyClaimHub.Controllers;

public sealed class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var products = await _context.MotorProducts
            .AsNoTracking()
            .OrderBy(product => product.ProductClass)
            .ThenBy(product => product.Name)
            .ToListAsync();

        return View(products);
    }
}
