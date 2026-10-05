using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolicyClaimHub.Data;
using PolicyClaimHub.Dtos.Policies;
using PolicyClaimHub.Models;

namespace PolicyClaimHub.Controllers.Api;

[ApiController]
[Route("api/policies")]
public sealed class PoliciesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PoliciesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType<List<PolicyResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PolicyResponseDto>>> GetAll()
    {
        var policies = await _context.InsurancePolicies
            .AsNoTracking()
            .OrderBy(policy => policy.PolicyNumber)
            .Select(policy => new PolicyResponseDto
            {
                Id = policy.Id,
                PolicyNumber = policy.PolicyNumber,
                InsuredName = policy.InsuredName,
                SumAssured = policy.SumAssured,
                PremiumAmount = policy.PremiumAmount,
                CoverageStartDate = policy.CoverageStartDate,
                CoverageEndDate = policy.CoverageEndDate,
                Status = policy.Status
            })
            .ToListAsync();

        return Ok(policies);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PolicyResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyResponseDto>> GetById(int id)
    {
        var policy = await _context.InsurancePolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (policy is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(policy));
    }

    [HttpPost]
    [ProducesResponseType<PolicyResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PolicyResponseDto>> Create(
        PolicyRequestDto request)
    {
        ValidateCoverageDates(request);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var policyNumber = NormalizePolicyNumber(request.PolicyNumber);
        var policyNumberExists = await _context.InsurancePolicies
            .AnyAsync(policy => policy.PolicyNumber == policyNumber);

        if (policyNumberExists)
        {
            return Conflict(new
            {
                message = "เลขกรมธรรม์นี้มีอยู่ในระบบแล้ว"
            });
        }

        var policy = new InsurancePolicy
        {
            PolicyNumber = policyNumber,
            InsuredName = request.InsuredName.Trim(),
            SumAssured = request.SumAssured,
            PremiumAmount = request.PremiumAmount,
            CoverageStartDate = request.CoverageStartDate,
            CoverageEndDate = request.CoverageEndDate,
            Status = request.Status
        };

        _context.InsurancePolicies.Add(policy);
        await _context.SaveChangesAsync();

        var response = ToResponse(policy);

        return CreatedAtAction(
            nameof(GetById),
            new { id = policy.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        PolicyRequestDto request)
    {
        ValidateCoverageDates(request);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var policy = await _context.InsurancePolicies.FindAsync(id);

        if (policy is null)
        {
            return NotFound();
        }

        var policyNumber = NormalizePolicyNumber(request.PolicyNumber);
        var policyNumberExists = await _context.InsurancePolicies
            .AnyAsync(item =>
                item.Id != id && item.PolicyNumber == policyNumber);

        if (policyNumberExists)
        {
            return Conflict(new
            {
                message = "เลขกรมธรรม์นี้มีอยู่ในระบบแล้ว"
            });
        }

        policy.PolicyNumber = policyNumber;
        policy.InsuredName = request.InsuredName.Trim();
        policy.SumAssured = request.SumAssured;
        policy.PremiumAmount = request.PremiumAmount;
        policy.CoverageStartDate = request.CoverageStartDate;
        policy.CoverageEndDate = request.CoverageEndDate;
        policy.Status = request.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var policy = await _context.InsurancePolicies.FindAsync(id);

        if (policy is null)
        {
            return NotFound();
        }

        _context.InsurancePolicies.Remove(policy);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private void ValidateCoverageDates(PolicyRequestDto request)
    {
        if (request.CoverageEndDate <= request.CoverageStartDate)
        {
            ModelState.AddModelError(
                nameof(request.CoverageEndDate),
                "วันที่สิ้นสุดต้องมากกว่าวันที่เริ่มคุ้มครอง");
        }
    }

    private static string NormalizePolicyNumber(string policyNumber)
    {
        return policyNumber.Trim().ToUpperInvariant();
    }

    private static PolicyResponseDto ToResponse(InsurancePolicy policy)
    {
        return new PolicyResponseDto
        {
            Id = policy.Id,
            PolicyNumber = policy.PolicyNumber,
            InsuredName = policy.InsuredName,
            SumAssured = policy.SumAssured,
            PremiumAmount = policy.PremiumAmount,
            CoverageStartDate = policy.CoverageStartDate,
            CoverageEndDate = policy.CoverageEndDate,
            Status = policy.Status
        };
    }
}
