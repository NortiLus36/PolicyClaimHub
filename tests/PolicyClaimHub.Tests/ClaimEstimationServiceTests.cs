using PolicyClaimHub.Models;
using PolicyClaimHub.Services.Claims;

namespace PolicyClaimHub.Tests;

public sealed class ClaimEstimationServiceTests
{
    private readonly ClaimEstimationService _service = new();

    [Fact]
    public void Estimate_ActivePolicyAndCoveredDate_ReturnsExpectedPayout()
    {
        var policy = CreateActivePolicy();
        var input = new ClaimEstimateInput(
            new DateTime(2026, 10, 5),
            65,
            700_000,
            10_000,
            25_000);

        var result = _service.Estimate(policy, input);

        Assert.True(result.IsEligible);
        Assert.Equal("สูง", result.Severity);
        Assert.Equal(0.60m, result.DamageRate);
        Assert.Equal(600_000m, result.GrossEstimatedLoss);
        Assert.Equal(565_000m, result.NetEstimatedPayout);
    }

    [Fact]
    public void Estimate_InactivePolicy_ReturnsIneligible()
    {
        var policy = CreateActivePolicy();
        policy.Status = PolicyStatus.Cancelled;

        var result = _service.Estimate(
            policy,
            CreateValidInput());

        Assert.False(result.IsEligible);
        Assert.Equal(0, result.NetEstimatedPayout);
        Assert.Contains("ไม่ได้อยู่ในสถานะ", result.Reason);
    }

    [Fact]
    public void Estimate_IncidentOutsideCoverage_ReturnsIneligible()
    {
        var policy = CreateActivePolicy();
        var input = CreateValidInput() with
        {
            IncidentDate = new DateTime(2028, 1, 1)
        };

        var result = _service.Estimate(policy, input);

        Assert.False(result.IsEligible);
        Assert.Equal(0, result.NetEstimatedPayout);
        Assert.Contains("นอกช่วงความคุ้มครอง", result.Reason);
    }

    [Fact]
    public void Estimate_DeductionsExceedGrossLoss_ReturnsZero()
    {
        var policy = CreateActivePolicy();
        var input = new ClaimEstimateInput(
            new DateTime(2026, 10, 5),
            10,
            100_000,
            80_000,
            50_000);

        var result = _service.Estimate(policy, input);

        Assert.True(result.IsEligible);
        Assert.Equal(0, result.NetEstimatedPayout);
    }

    [Theory]
    [InlineData(10, "ต่ำ", 0.15)]
    [InlineData(30, "ปานกลาง", 0.35)]
    [InlineData(65, "สูง", 0.60)]
    [InlineData(120, "รุนแรง", 0.85)]
    public void Estimate_WaterDepth_SelectsExpectedRate(
        decimal waterDepthCm,
        string expectedSeverity,
        decimal expectedRate)
    {
        var policy = CreateActivePolicy();
        var input = CreateValidInput() with
        {
            WaterDepthCm = waterDepthCm
        };

        var result = _service.Estimate(policy, input);

        Assert.Equal(expectedSeverity, result.Severity);
        Assert.Equal(expectedRate, result.DamageRate);
    }

    private static InsurancePolicy CreateActivePolicy()
    {
        return new InsurancePolicy
        {
            PolicyNumber = "TEST-001",
            InsuredName = "Unit Test",
            SumAssured = 1_000_000,
            PremiumAmount = 12_000,
            CoverageStartDate = new DateTime(2026, 1, 1),
            CoverageEndDate = new DateTime(2027, 12, 31),
            Status = PolicyStatus.Active
        };
    }

    private static ClaimEstimateInput CreateValidInput()
    {
        return new ClaimEstimateInput(
            new DateTime(2026, 10, 5),
            65,
            700_000,
            10_000,
            25_000);
    }
}
