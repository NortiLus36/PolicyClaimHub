using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using PolicyClaimHub.Data;
using PolicyClaimHub.Models;
using PolicyClaimHub.ViewModels;

namespace PolicyClaimHub.Controllers;

public class PoliciesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PoliciesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        var query = _context.InsurancePolicies
            .AsNoTracking()
            .Include(policy => policy.MotorProduct)
            .Include(policy => policy.ClaimHistories);
        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        var policies = await query
            .OrderBy(policy => policy.PolicyNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(new PagedResultViewModel<InsurancePolicy>
        {
            Items = policies,
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var policy = await _context.InsurancePolicies
            .AsNoTracking()
            .Include(item => item.MotorProduct)
            .Include(item => item.ClaimHistories)
            .FirstOrDefaultAsync(item => item.Id == id.Value);

        if (policy is null)
        {
            return NotFound();
        }

        return View(policy);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateProductsAsync();
        return View(new InsurancePolicy());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(
            "PolicyNumber,InsuredName,CustomerType,PhoneNumber,MotorProductId," +
            "VehicleRegistration,VehicleMake,VehicleModel,VehicleYear," +
            "SumAssured,PremiumAmount," +
            "CoverageStartDate,CoverageEndDate,Status")]
        InsurancePolicy policy)
    {
        NormalizePolicy(policy);
        ValidateCoverageDates(policy);

        if (!ModelState.IsValid)
        {
            await PopulateProductsAsync(policy.MotorProductId);
            return View(policy);
        }

        var policyNumberExists = await _context.InsurancePolicies
            .AnyAsync(item => item.PolicyNumber == policy.PolicyNumber);

        if (policyNumberExists)
        {
            ModelState.AddModelError(
                nameof(policy.PolicyNumber),
                "เลขกรมธรรม์นี้มีอยู่ในระบบแล้ว");

            await PopulateProductsAsync(policy.MotorProductId);
            return View(policy);
        }

        _context.InsurancePolicies.Add(policy);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "เพิ่มกรมธรรม์สำเร็จ";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var policy = await _context.InsurancePolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id.Value);

        if (policy is null)
        {
            return NotFound();
        }

        await PopulateProductsAsync(policy.MotorProductId);

        return View(policy);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind(
            "Id,PolicyNumber,InsuredName,CustomerType,PhoneNumber,MotorProductId," +
            "VehicleRegistration,VehicleMake,VehicleModel,VehicleYear," +
            "SumAssured,PremiumAmount," +
            "CoverageStartDate,CoverageEndDate,Status")]
        InsurancePolicy policy)
    {
        if (id != policy.Id)
        {
            return NotFound();
        }

        NormalizePolicy(policy);
        ValidateCoverageDates(policy);

        if (!ModelState.IsValid)
        {
            await PopulateProductsAsync(policy.MotorProductId);
            return View(policy);
        }

        var policyNumberExists = await _context.InsurancePolicies
            .AnyAsync(item =>
                item.Id != policy.Id &&
                item.PolicyNumber == policy.PolicyNumber);

        if (policyNumberExists)
        {
            ModelState.AddModelError(
                nameof(policy.PolicyNumber),
                "เลขกรมธรรม์นี้มีอยู่ในระบบแล้ว");

            await PopulateProductsAsync(policy.MotorProductId);
            return View(policy);
        }

        _context.InsurancePolicies.Update(policy);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await PolicyExistsAsync(policy.Id))
            {
                return NotFound();
            }

            throw;
        }

        TempData["SuccessMessage"] = "แก้ไขกรมธรรม์สำเร็จ";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var policy = await _context.InsurancePolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id.Value);

        if (policy is null)
        {
            return NotFound();
        }

        return View(policy);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var policy = await _context.InsurancePolicies.FindAsync(id);

        if (policy is null)
        {
            return NotFound();
        }

        _context.InsurancePolicies.Remove(policy);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "ลบกรมธรรม์สำเร็จ";

        return RedirectToAction(nameof(Index));
    }

    private void ValidateCoverageDates(InsurancePolicy policy)
    {
        if (policy.CoverageEndDate <= policy.CoverageStartDate)
        {
            ModelState.AddModelError(
                nameof(policy.CoverageEndDate),
                "วันที่สิ้นสุดต้องมากกว่าวันที่เริ่มคุ้มครอง");
        }
    }

    private static void NormalizePolicy(InsurancePolicy policy)
    {
        policy.PolicyNumber =
            (policy.PolicyNumber ?? string.Empty).Trim().ToUpperInvariant();

        policy.InsuredName =
            (policy.InsuredName ?? string.Empty).Trim();

        policy.PhoneNumber = (policy.PhoneNumber ?? string.Empty).Trim();
        policy.VehicleRegistration =
            (policy.VehicleRegistration ?? string.Empty).Trim().ToUpperInvariant();
        policy.VehicleMake = (policy.VehicleMake ?? string.Empty).Trim();
        policy.VehicleModel = (policy.VehicleModel ?? string.Empty).Trim();
    }

    private Task<bool> PolicyExistsAsync(int id)
    {
        return _context.InsurancePolicies.AnyAsync(item => item.Id == id);
    }

    private async Task PopulateProductsAsync(int? selectedId = null)
    {
        var products = await _context.MotorProducts
            .AsNoTracking()
            .Where(product => product.IsActive)
            .OrderBy(product => product.ProductClass)
            .ThenBy(product => product.Name)
            .ToListAsync();

        ViewBag.MotorProducts = new SelectList(
            products,
            nameof(MotorProduct.Id),
            nameof(MotorProduct.Name),
            selectedId);
    }
}
