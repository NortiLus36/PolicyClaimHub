namespace PolicyClaimHub.Services.OracleClaims;

public interface IOracleFloodClaimService
{
    bool IsConfigured { get; }

    Task<OracleDatabaseStatus> GetStatusAsync(
        CancellationToken cancellationToken = default);

    Task<OracleClaimPage> GetPageAsync(
        int pageNumber,
        int pageSize,
        string? district,
        CancellationToken cancellationToken = default);

    Task<OracleSubmitClaimResult> SubmitAsync(
        OracleSubmitClaimCommand request,
        CancellationToken cancellationToken = default);

    Task ApproveAsync(
        OracleApproveClaimCommand request,
        CancellationToken cancellationToken = default);
}
