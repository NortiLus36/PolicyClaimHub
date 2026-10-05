using PolicyClaimHub.Models;

namespace PolicyClaimHub.Services.Claims;

public sealed class ClaimEstimationService : IClaimEstimationService
{
    public ClaimEstimateResult Estimate(
        InsurancePolicy policy,
        ClaimEstimateInput input)
    {
        if (policy.Status != PolicyStatus.Active)
        {
            return Ineligible("กรมธรรม์ไม่ได้อยู่ในสถานะมีผลคุ้มครอง", input);
        }

        var incidentDate = input.IncidentDate.Date;

        if (incidentDate < policy.CoverageStartDate.Date ||
            incidentDate > policy.CoverageEndDate.Date)
        {
            return Ineligible("วันที่เกิดเหตุอยู่นอกช่วงความคุ้มครอง", input);
        }

        var (severity, damageRate) = GetDamageRate(input.WaterDepthCm);
        var maximumLossByDepth = policy.SumAssured * damageRate;
        var grossEstimatedLoss = Math.Min(
            input.RequestedAmount,
            maximumLossByDepth);

        var netEstimatedPayout = Math.Max(
            0,
            grossEstimatedLoss -
            input.DeductibleAmount -
            input.OutstandingDebt);

        netEstimatedPayout = Math.Min(
            netEstimatedPayout,
            policy.SumAssured);

        return new ClaimEstimateResult(
            true,
            "ผ่านเงื่อนไขเบื้องต้นสำหรับการประเมิน",
            severity,
            damageRate,
            grossEstimatedLoss,
            input.DeductibleAmount,
            input.OutstandingDebt,
            netEstimatedPayout);
    }

    private static (string Severity, decimal Rate) GetDamageRate(
        decimal waterDepthCm)
    {
        return waterDepthCm switch
        {
            < 20 => ("ต่ำ", 0.15m),
            < 50 => ("ปานกลาง", 0.35m),
            < 100 => ("สูง", 0.60m),
            _ => ("รุนแรง", 0.85m)
        };
    }

    private static ClaimEstimateResult Ineligible(
        string reason,
        ClaimEstimateInput input)
    {
        return new ClaimEstimateResult(
            false,
            reason,
            "ไม่เข้าเกณฑ์",
            0,
            0,
            input.DeductibleAmount,
            input.OutstandingDebt,
            0);
    }
}
