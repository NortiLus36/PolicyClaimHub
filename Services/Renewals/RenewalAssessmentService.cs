using PolicyClaimHub.Models;

namespace PolicyClaimHub.Services.Renewals;

public sealed class RenewalAssessmentService : IRenewalAssessmentService
{
    public RenewalAssessment Assess(InsurancePolicy policy)
    {
        var claims = policy.ClaimHistories ?? [];
        var claimCount = claims.Count;
        var totalClaimAmount = claims.Sum(claim => claim.PaidAmount);
        var lossRatio = policy.PremiumAmount > 0
            ? totalClaimAmount / policy.PremiumAmount
            : 0;

        var score = policy.CustomerType == CustomerType.Existing ? 78 : 62;

        score += claimCount == 0 ? 12 : -(claimCount * 8);
        score += policy.Status == PolicyStatus.Active ? 5 : -15;

        var daysUntilExpiry = (policy.CoverageEndDate.Date - DateTime.Today).Days;
        if (daysUntilExpiry is >= 0 and <= 60)
        {
            score += 5;
        }

        if (lossRatio >= 40)
        {
            score -= 20;
        }
        else if (lossRatio >= 20)
        {
            score -= 10;
        }

        score = Math.Clamp(score, 5, 95);

        var riskLevel = DetermineRiskLevel(claimCount, lossRatio);
        var premiumMultiplier = riskLevel switch
        {
            CustomerRiskLevel.Low => 0.90m,
            CustomerRiskLevel.Standard => 1.00m,
            CustomerRiskLevel.Watchlist => 1.15m,
            CustomerRiskLevel.High => 1.30m,
            _ => 1.00m
        };

        var recommendedPremium = Math.Round(
            policy.PremiumAmount * premiumMultiplier,
            0,
            MidpointRounding.AwayFromZero);

        return new RenewalAssessment(
            claimCount,
            totalClaimAmount,
            score,
            riskLevel,
            recommendedPremium,
            BuildRecommendation(policy, claimCount, riskLevel, daysUntilExpiry));
    }

    private static CustomerRiskLevel DetermineRiskLevel(
        int claimCount,
        decimal lossRatio)
    {
        if (claimCount >= 4 || lossRatio >= 40)
        {
            return CustomerRiskLevel.High;
        }

        if (claimCount >= 2 || lossRatio >= 20)
        {
            return CustomerRiskLevel.Watchlist;
        }

        return claimCount == 0
            ? CustomerRiskLevel.Low
            : CustomerRiskLevel.Standard;
    }

    private static string BuildRecommendation(
        InsurancePolicy policy,
        int claimCount,
        CustomerRiskLevel riskLevel,
        int daysUntilExpiry)
    {
        if (policy.Status == PolicyStatus.Cancelled)
        {
            return "ตรวจสอบสาเหตุการยกเลิกก่อนเสนอขายใหม่";
        }

        if (riskLevel == CustomerRiskLevel.High)
        {
            return "ส่งเจ้าหน้าที่พิจารณาประวัติเคลมและเงื่อนไขรับประกัน";
        }

        if (riskLevel == CustomerRiskLevel.Watchlist)
        {
            return "พิจารณาปรับเบี้ยและ Deductible ก่อนเสนอการต่ออายุ";
        }

        if (daysUntilExpiry is >= 0 and <= 60)
        {
            return claimCount == 0
                ? "ลูกค้าประวัติดี ควรติดต่อต่ออายุและเสนอส่วนลด"
                : "ควรติดต่อต่ออายุและทบทวนประวัติเคลม";
        }

        return claimCount == 0
            ? "รักษาความสัมพันธ์และติดตามก่อนหมดอายุ"
            : "ติดตามตามรอบและประเมินประวัติเคลมอีกครั้ง";
    }
}
