using PolicyClaimHub.Models;

namespace PolicyClaimHub.Services.Renewals;

public interface IRenewalAssessmentService
{
    RenewalAssessment Assess(InsurancePolicy policy);
}
