using Microsoft.AspNetCore.Mvc;
using PolicyClaimHub.Dtos.OracleClaims;
using PolicyClaimHub.Services.OracleClaims;
using PolicyClaimHub.ViewModels;

namespace PolicyClaimHub.Controllers;

public sealed class OracleClaimsController : Controller
{
    private readonly IOracleFloodClaimService _service;

    public OracleClaimsController(IOracleFloodClaimService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int pageNumber = 1,
        string? district = null,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 10;
        var status = await _service.GetStatusAsync(cancellationToken);
        var claims = new OracleClaimPage(
            [],
            Math.Max(1, pageNumber),
            pageSize,
            0);

        if (status.CanConnect && status.PackageStatus == "VALID")
        {
            try
            {
                claims = await _service.GetPageAsync(
                    Math.Max(1, pageNumber),
                    pageSize,
                    district,
                    cancellationToken);
            }
            catch (Exception exception) when (
                exception is OraclePackageException or OracleIntegrationException)
            {
                TempData["ErrorMessage"] = exception.Message;
            }
        }

        return View(new OracleClaimsIndexViewModel
        {
            DatabaseStatus = status,
            Claims = claims,
            District = district
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new OracleSubmitClaimRequestDto
        {
            PolicyNumber = "ORA-DEMO-001",
            IncidentDate = DateTime.Today,
            District = "สาทร",
            Latitude = 13.720m,
            Longitude = 100.533m,
            WaterDepthCm = 65,
            RequestedAmount = 700_000,
            DeductibleAmount = 10_000,
            OutstandingDebt = 25_000
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        OracleSubmitClaimRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            var result = await _service.SubmitAsync(
                ToCommand(request),
                cancellationToken);
            TempData["SuccessMessage"] =
                $"ยื่นเคลมสำเร็จ Claim ID {result.ClaimId:N0} ยอดประเมิน {result.EstimatedPayout:N2} บาท";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (
            exception is OraclePackageException or
                OracleIntegrationException or
                InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(
        long claimId,
        decimal approvedAmount,
        string? changeNote,
        CancellationToken cancellationToken)
    {
        if (approvedAmount < 0)
        {
            TempData["ErrorMessage"] = "ยอดอนุมัติต้องไม่ติดลบ";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _service.ApproveAsync(
                new OracleApproveClaimCommand(
                    claimId,
                    approvedAmount,
                    "PORTFOLIO_APPROVER",
                    changeNote),
                cancellationToken);
            TempData["SuccessMessage"] = $"อนุมัติเคลม ID {claimId:N0} สำเร็จ";
        }
        catch (Exception exception) when (
            exception is OraclePackageException or
                OracleIntegrationException or
                InvalidOperationException)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
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
