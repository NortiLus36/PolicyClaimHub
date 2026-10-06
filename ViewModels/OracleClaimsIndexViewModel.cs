using PolicyClaimHub.Services.OracleClaims;

namespace PolicyClaimHub.ViewModels;

public sealed class OracleClaimsIndexViewModel
{
    public required OracleDatabaseStatus DatabaseStatus { get; init; }

    public required OracleClaimPage Claims { get; init; }

    public string? District { get; init; }
}
