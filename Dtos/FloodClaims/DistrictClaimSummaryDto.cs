namespace PolicyClaimHub.Dtos.FloodClaims;

public sealed class DistrictClaimSummaryDto
{
    public string District { get; init; } = string.Empty;
    public int ClaimCount { get; init; }
    public decimal TotalRequestedAmount { get; init; }
    public decimal TotalEstimatedPayout { get; init; }
}
