namespace PolicyClaimHub.Services.OracleClaims;

public sealed record OracleSubmitClaimCommand(
    string ClaimNumber,
    string PolicyNumber,
    string VehicleRegistration,
    DateTime IncidentDate,
    string District,
    decimal Latitude,
    decimal Longitude,
    decimal WaterDepthCm,
    decimal RequestedAmount,
    decimal DeductibleAmount,
    decimal OutstandingDebt,
    string ChangedBy);

public sealed record OracleSubmitClaimResult(
    long ClaimId,
    decimal EstimatedPayout);

public sealed record OracleApproveClaimCommand(
    long ClaimId,
    decimal ApprovedAmount,
    string ChangedBy,
    string? ChangeNote);

public sealed record OracleClaimPageItem(
    long ClaimId,
    string ClaimNumber,
    string PolicyNumber,
    string VehicleRegistration,
    DateTime IncidentDate,
    string District,
    decimal WaterDepthCm,
    decimal RequestedAmount,
    decimal EstimatedPayout,
    string ClaimStatus);

public sealed record OracleClaimPage(
    IReadOnlyList<OracleClaimPageItem> Items,
    int PageNumber,
    int PageSize,
    int TotalRows)
{
    public int TotalPages => Math.Max(
        1,
        (int)Math.Ceiling(TotalRows / (double)PageSize));
}

public sealed record OracleDatabaseStatus(
    bool IsConfigured,
    bool CanConnect,
    string PackageStatus,
    string Message);
