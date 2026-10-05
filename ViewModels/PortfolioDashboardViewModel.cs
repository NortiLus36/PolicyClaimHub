using PolicyClaimHub.Models;
using PolicyClaimHub.Services.Renewals;

namespace PolicyClaimHub.ViewModels;

public sealed class PortfolioDashboardViewModel
{
    public int TotalCustomers { get; init; }
    public int NewCustomers { get; init; }
    public int RenewalCandidates { get; init; }
    public int HighRiskCustomers { get; init; }
    public decimal TotalPremium { get; init; }
    public IReadOnlyList<PortfolioCustomerRowViewModel> Customers { get; init; }
        = [];
}

public sealed class PortfolioCustomerRowViewModel
{
    public required InsurancePolicy Policy { get; init; }
    public required RenewalAssessment Assessment { get; init; }
}
