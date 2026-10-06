using Microsoft.AspNetCore.Mvc;
using PolicyClaimHub.Dtos.OracleClaims;
using PolicyClaimHub.Services.OracleClaims;

namespace PolicyClaimHub.Controllers.Api;

[ApiController]
[Route("api/oracle-flood-claims")]
public sealed class OracleFloodClaimsApiController : ControllerBase
{
    private readonly IOracleFloodClaimService _service;

    public OracleFloodClaimsApiController(IOracleFloodClaimService service)
    {
        _service = service;
    }

    [HttpGet("health")]
    [ProducesResponseType<OracleDatabaseStatus>(StatusCodes.Status200OK)]
    [ProducesResponseType<OracleDatabaseStatus>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OracleDatabaseStatus>> GetHealth(
        CancellationToken cancellationToken)
    {
        var status = await _service.GetStatusAsync(cancellationToken);
        return status.CanConnect
            ? Ok(status)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, status);
    }

    [HttpGet]
    [ProducesResponseType<OracleClaimPage>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OracleClaimPage>> GetPage(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? district = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _service.GetPageAsync(
                pageNumber,
                pageSize,
                district,
                cancellationToken));
        }
        catch (Exception exception)
        {
            return ToErrorResult(exception);
        }
    }

    [HttpPost]
    [ProducesResponseType<OracleSubmitClaimResult>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<OracleSubmitClaimResult>> Submit(
        OracleSubmitClaimRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.SubmitAsync(
                ToCommand(request),
                cancellationToken);
            return Created(
                $"/api/oracle-flood-claims?pageNumber=1&pageSize=10",
                result);
        }
        catch (Exception exception)
        {
            return ToErrorResult(exception);
        }
    }

    [HttpPut("{claimId:long}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Approve(
        long claimId,
        OracleApproveClaimRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.ApproveAsync(
                new OracleApproveClaimCommand(
                    claimId,
                    request.ApprovedAmount,
                    request.ChangedBy,
                    request.ChangeNote),
                cancellationToken);
            return NoContent();
        }
        catch (Exception exception)
        {
            return ToErrorResult(exception);
        }
    }

    private ActionResult ToErrorResult(Exception exception)
    {
        return exception switch
        {
            OraclePackageException { ErrorNumber: 20003 or 20007 } error =>
                Problem(error.Message, statusCode: StatusCodes.Status404NotFound),
            OraclePackageException { ErrorNumber: 20004 } error =>
                Problem(error.Message, statusCode: StatusCodes.Status409Conflict),
            OraclePackageException error =>
                Problem(error.Message, statusCode: StatusCodes.Status400BadRequest),
            InvalidOperationException error =>
                Problem(error.Message, statusCode: StatusCodes.Status503ServiceUnavailable),
            OracleIntegrationException error =>
                Problem(error.Message, statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => Problem(
                "เกิดข้อผิดพลาดที่ไม่คาดคิด",
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    private static OracleSubmitClaimCommand ToCommand(
        OracleSubmitClaimRequestDto request)
    {
        return new OracleSubmitClaimCommand(
            request.ClaimNumber,
            request.PolicyNumber,
            request.VehicleRegistration,
            request.IncidentDate,
            request.District,
            request.Latitude,
            request.Longitude,
            request.WaterDepthCm,
            request.RequestedAmount,
            request.DeductibleAmount,
            request.OutstandingDebt,
            request.ChangedBy);
    }
}
