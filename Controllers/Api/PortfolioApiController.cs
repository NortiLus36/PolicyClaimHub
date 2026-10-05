using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolicyClaimHub.Data;
using PolicyClaimHub.Services.Renewals;

namespace PolicyClaimHub.Controllers.Api;

[ApiController]
[Route("api/portfolio")]
public sealed class PortfolioApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IRenewalAssessmentService _assessmentService;

    public PortfolioApiController(ApplicationDbContext context, IRenewalAssessmentService assessmentService)
    {
        _context = context;
        _assessmentService = assessmentService;
    }

    [HttpGet("renewal-insights")]
    public async Task<IActionResult> GetRenewalInsights()
    {
        var policies = await _context.InsurancePolicies
            .AsNoTracking()
            .Include(policy => policy.MotorProduct)
            .Include(policy => policy.ClaimHistories)
            .Where(policy => policy.PolicyNumber.StartsWith("PORT-"))
            .OrderBy(policy => policy.CoverageEndDate)
            .ToListAsync();

        var response = policies.Select(policy =>
        {
            var assessment = _assessmentService.Assess(policy);
            return new
            {
                policy.Id,
                policy.PolicyNumber,
                policy.InsuredName,
                customerType = policy.CustomerType.ToString(),
                product = policy.MotorProduct?.Name,
                policy.VehicleRegistration,
                policy.PremiumAmount,
                policy.CoverageEndDate,
                assessment.ClaimCount,
                assessment.TotalClaimAmount,
                riskLevel = assessment.RiskLevel.ToString(),
                assessment.RenewalProbabilityPercent,
                assessment.RecommendedPremium,
                assessment.Recommendation
            };
        });

        return Ok(response);
    }
}
