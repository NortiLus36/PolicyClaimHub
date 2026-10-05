namespace PolicyClaimHub.Services.Renewals;

public enum CustomerRiskLevel
{
    Low = 1,
    Standard = 2,
    Watchlist = 3,
    High = 4
}

public sealed record RenewalAssessment(
    int ClaimCount,
    decimal TotalClaimAmount,
    int RenewalProbabilityPercent,
    CustomerRiskLevel RiskLevel,
    decimal RecommendedPremium,
    string Recommendation);
