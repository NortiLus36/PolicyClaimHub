using PolicyClaimHub.Models;
using PolicyClaimHub.Services.Renewals;

namespace PolicyClaimHub.Tests;

public sealed class RenewalAssessmentServiceTests
{
    private readonly RenewalAssessmentService _service = new();

    [Fact]
    public void Assess_NoClaims_ReturnsLowRiskAndDiscountedPremium()
    {
        var policy = CreatePolicy();

        var result = _service.Assess(policy);

        Assert.Equal(CustomerRiskLevel.Low, result.RiskLevel);
        Assert.Equal(0, result.ClaimCount);
        Assert.Equal(13_500m, result.RecommendedPremium);
        Assert.True(result.RenewalProbabilityPercent >= 70);
    }

    [Fact]
    public void Assess_FourClaims_ReturnsHighRiskAndHigherPremium()
    {
        var policy = CreatePolicy();
        policy.ClaimHistories = Enumerable.Range(1, 4)
            .Select(index => new ClaimHistory
            {
                ClaimNumber = $"TEST-{index}",
                PaidAmount = 80_000m,
                ClaimType = ClaimType.Collision
            })
            .ToList();

        var result = _service.Assess(policy);

        Assert.Equal(CustomerRiskLevel.High, result.RiskLevel);
        Assert.Equal(4, result.ClaimCount);
        Assert.Equal(19_500m, result.RecommendedPremium);
        Assert.Contains("พิจารณา", result.Recommendation);
    }

    [Fact]
    public void Assess_NewCustomer_HasLowerRenewalScoreThanExistingCustomer()
    {
        var existing = CreatePolicy();
        var newcomer = CreatePolicy();
        newcomer.CustomerType = CustomerType.New;

        var existingResult = _service.Assess(existing);
        var newcomerResult = _service.Assess(newcomer);

        Assert.True(existingResult.RenewalProbabilityPercent > newcomerResult.RenewalProbabilityPercent);
    }

    private static InsurancePolicy CreatePolicy()
    {
        return new InsurancePolicy
        {
            PolicyNumber = "TEST-001",
            InsuredName = "ลูกค้าทดสอบ",
            CustomerType = CustomerType.Existing,
            VehicleRegistration = "กข-1234",
            VehicleMake = "Toyota",
            VehicleModel = "Yaris",
            VehicleYear = 2022,
            SumAssured = 600_000m,
            PremiumAmount = 15_000m,
            CoverageStartDate = DateTime.Today.AddMonths(-11),
            CoverageEndDate = DateTime.Today.AddMonths(1),
            Status = PolicyStatus.Active
        };
    }
}
