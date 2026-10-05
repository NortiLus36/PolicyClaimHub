using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolicyClaimHub.Data;
using PolicyClaimHub.Models;
using PolicyClaimHub.Services.Renewals;
using PolicyClaimHub.ViewModels;

namespace PolicyClaimHub.Controllers;

public sealed class PortfolioController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IRenewalAssessmentService _assessmentService;

    public PortfolioController(
        ApplicationDbContext context,
        IRenewalAssessmentService assessmentService)
    {
        _context = context;
        _assessmentService = assessmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? risk, string? search)
    {
        var policies = await _context.InsurancePolicies
            .AsNoTracking()
            .Include(policy => policy.MotorProduct)
            .Include(policy => policy.ClaimHistories)
            .Where(policy => policy.PolicyNumber.StartsWith("PORT-"))
            .OrderBy(policy => policy.CoverageEndDate)
            .ToListAsync();

        var rows = policies
            .Select(policy => new PortfolioCustomerRowViewModel
            {
                Policy = policy,
                Assessment = _assessmentService.Assess(policy)
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            rows = rows.Where(row =>
                    row.Policy.InsuredName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    row.Policy.PolicyNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    row.Policy.VehicleRegistration.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (Enum.TryParse<CustomerRiskLevel>(risk, true, out var riskLevel))
        {
            rows = rows.Where(row => row.Assessment.RiskLevel == riskLevel).ToList();
        }

        ViewBag.Search = search;
        ViewBag.Risk = risk;

        var allAssessments = policies.Select(_assessmentService.Assess).ToList();
        var model = new PortfolioDashboardViewModel
        {
            TotalCustomers = policies.Count,
            NewCustomers = policies.Count(policy => policy.CustomerType == CustomerType.New),
            RenewalCandidates = allAssessments.Count(item => item.RenewalProbabilityPercent >= 70),
            HighRiskCustomers = allAssessments.Count(item => item.RiskLevel == CustomerRiskLevel.High),
            TotalPremium = policies.Sum(policy => policy.PremiumAmount),
            Customers = rows
        };

        return View(model);
    }
}
