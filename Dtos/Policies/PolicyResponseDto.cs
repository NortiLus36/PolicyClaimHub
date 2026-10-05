using PolicyClaimHub.Models;

namespace PolicyClaimHub.Dtos.Policies;

public sealed class PolicyResponseDto
{
    public int Id { get; init; }
    public string PolicyNumber { get; init; } = string.Empty;
    public string InsuredName { get; init; } = string.Empty;
    public decimal SumAssured { get; init; }
    public decimal PremiumAmount { get; init; }
    public DateTime CoverageStartDate { get; init; }
    public DateTime CoverageEndDate { get; init; }
    public PolicyStatus Status { get; init; }
}
