namespace PolicyClaimHub.Services.Claims;

public sealed record ClaimEstimateInput(
    DateTime IncidentDate,
    decimal WaterDepthCm,
    decimal RequestedAmount,
    decimal DeductibleAmount,
    decimal OutstandingDebt);
