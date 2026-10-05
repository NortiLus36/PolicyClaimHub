using PolicyClaimHub.Models;

namespace PolicyClaimHub.Services.Claims;

public interface IClaimEstimationService
{
    ClaimEstimateResult Estimate(
        InsurancePolicy policy,
        ClaimEstimateInput input);
}
