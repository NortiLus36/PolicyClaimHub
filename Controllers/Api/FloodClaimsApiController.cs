using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolicyClaimHub.Data;
using PolicyClaimHub.Dtos.FloodClaims;
using PolicyClaimHub.Models;
using PolicyClaimHub.Services.Claims;

namespace PolicyClaimHub.Controllers.Api;

[ApiController]
[Route("api/flood-claims")]
public sealed class FloodClaimsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IClaimEstimationService _estimationService;

    public FloodClaimsApiController(
        ApplicationDbContext context,
        IClaimEstimationService estimationService)
    {
        _context = context;
        _estimationService = estimationService;
    }

    [HttpGet]
    [ProducesResponseType<List<FloodClaimResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FloodClaimResponseDto>>> GetAll()
    {
        var claims = await _context.MotorFloodClaims
            .AsNoTracking()
            .OrderByDescending(claim => claim.IncidentDate)
            .Select(claim => new FloodClaimResponseDto
            {
                Id = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                PolicyId = claim.InsurancePolicyId,
                PolicyNumber = claim.InsurancePolicy!.PolicyNumber,
                VehicleRegistration = claim.VehicleRegistration,
                IncidentDate = claim.IncidentDate,
                District = claim.District,
                Latitude = claim.Latitude,
                Longitude = claim.Longitude,
                WaterDepthCm = claim.WaterDepthCm,
                RequestedAmount = claim.RequestedAmount,
                EstimatedPayout = claim.EstimatedPayout,
                Status = claim.Status
            })
            .ToListAsync();

        return Ok(claims);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<FloodClaimResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FloodClaimResponseDto>> GetById(int id)
    {
        var claim = await _context.MotorFloodClaims
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new FloodClaimResponseDto
            {
                Id = item.Id,
                ClaimNumber = item.ClaimNumber,
                PolicyId = item.InsurancePolicyId,
                PolicyNumber = item.InsurancePolicy!.PolicyNumber,
                VehicleRegistration = item.VehicleRegistration,
                IncidentDate = item.IncidentDate,
                District = item.District,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                WaterDepthCm = item.WaterDepthCm,
                RequestedAmount = item.RequestedAmount,
                EstimatedPayout = item.EstimatedPayout,
                Status = item.Status
            })
            .FirstOrDefaultAsync();

        if (claim is null)
        {
            return NotFound();
        }

        return Ok(claim);
    }

    [HttpPost("estimate")]
    [ProducesResponseType<ClaimEstimateResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ClaimEstimateResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimEstimateResult>> Estimate(
        FloodClaimEstimateRequestDto request)
    {
        var policy = await _context.InsurancePolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.PolicyId);

        if (policy is null)
        {
            return NotFound(new { message = "ไม่พบกรมธรรม์" });
        }

        var result = _estimationService.Estimate(
            policy,
            ToEstimateInput(request));

        if (!result.IsEligible)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<FloodClaimResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FloodClaimResponseDto>> Create(
        CreateFloodClaimRequestDto request)
    {
        var claimNumber = request.ClaimNumber.Trim().ToUpperInvariant();
        var claimNumberExists = await _context.MotorFloodClaims
            .AnyAsync(claim => claim.ClaimNumber == claimNumber);

        if (claimNumberExists)
        {
            return Conflict(new { message = "เลขที่เคลมนี้มีอยู่ในระบบแล้ว" });
        }

        var policy = await _context.InsurancePolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.PolicyId);

        if (policy is null)
        {
            return NotFound(new { message = "ไม่พบกรมธรรม์" });
        }

        var estimate = _estimationService.Estimate(
            policy,
            ToEstimateInput(request));

        if (!estimate.IsEligible)
        {
            return BadRequest(estimate);
        }

        var claim = new MotorFloodClaim
        {
            ClaimNumber = claimNumber,
            InsurancePolicyId = policy.Id,
            VehicleRegistration = request.VehicleRegistration.Trim().ToUpperInvariant(),
            IncidentDate = request.IncidentDate,
            District = request.District.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            WaterDepthCm = request.WaterDepthCm,
            RequestedAmount = request.RequestedAmount,
            DeductibleAmount = request.DeductibleAmount,
            OutstandingDebt = request.OutstandingDebt,
            EstimatedPayout = estimate.NetEstimatedPayout,
            Status = FloodClaimStatus.Submitted
        };

        _context.MotorFloodClaims.Add(claim);
        await _context.SaveChangesAsync();

        var response = ToResponse(claim, policy.PolicyNumber);

        return CreatedAtAction(
            nameof(GetById),
            new { id = claim.Id },
            response);
    }

    [HttpGet("summary")]
    [ProducesResponseType<List<DistrictClaimSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DistrictClaimSummaryDto>>> GetSummary(
        [FromQuery] string? district)
    {
        var query = _context.MotorFloodClaims.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(district))
        {
            var normalizedDistrict = district.Trim();
            query = query.Where(claim => claim.District == normalizedDistrict);
        }

        var claims = await query.ToListAsync();
        var summary = claims
            .GroupBy(claim => claim.District)
            .Select(group => new DistrictClaimSummaryDto
            {
                District = group.Key,
                ClaimCount = group.Count(),
                TotalRequestedAmount = group.Sum(claim => claim.RequestedAmount),
                TotalEstimatedPayout = group.Sum(claim => claim.EstimatedPayout)
            })
            .OrderByDescending(item => item.TotalEstimatedPayout)
            .ToList();

        return Ok(summary);
    }

    private static ClaimEstimateInput ToEstimateInput(
        FloodClaimEstimateRequestDto request)
    {
        return new ClaimEstimateInput(
            request.IncidentDate,
            request.WaterDepthCm,
            request.RequestedAmount,
            request.DeductibleAmount,
            request.OutstandingDebt);
    }

    private static FloodClaimResponseDto ToResponse(
        MotorFloodClaim claim,
        string policyNumber)
    {
        return new FloodClaimResponseDto
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            PolicyId = claim.InsurancePolicyId,
            PolicyNumber = policyNumber,
            VehicleRegistration = claim.VehicleRegistration,
            IncidentDate = claim.IncidentDate,
            District = claim.District,
            Latitude = claim.Latitude,
            Longitude = claim.Longitude,
            WaterDepthCm = claim.WaterDepthCm,
            RequestedAmount = claim.RequestedAmount,
            EstimatedPayout = claim.EstimatedPayout,
            Status = claim.Status
        };
    }
}
