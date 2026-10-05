using PolicyClaimHub.Models;

namespace PolicyClaimHub.Dtos.FloodClaims;

public sealed class FloodClaimResponseDto
{
    public int Id { get; init; }
    public string ClaimNumber { get; init; } = string.Empty;
    public int PolicyId { get; init; }
    public string PolicyNumber { get; init; } = string.Empty;
    public string VehicleRegistration { get; init; } = string.Empty;
    public DateTime IncidentDate { get; init; }
    public string District { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public decimal WaterDepthCm { get; init; }
    public decimal RequestedAmount { get; init; }
    public decimal EstimatedPayout { get; init; }
    public FloodClaimStatus Status { get; init; }
}
