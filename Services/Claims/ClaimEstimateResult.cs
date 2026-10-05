namespace PolicyClaimHub.Services.Claims;

public sealed record ClaimEstimateResult(
    bool IsEligible,
    string Reason,
    string Severity,
    decimal DamageRate,
    decimal GrossEstimatedLoss,
    decimal DeductibleAmount,
    decimal OutstandingDebt,
    decimal NetEstimatedPayout);
