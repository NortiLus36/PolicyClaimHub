using Microsoft.EntityFrameworkCore;
using PolicyClaimHub.Models;

namespace PolicyClaimHub.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<InsurancePolicy> InsurancePolicies
        => Set<InsurancePolicy>();
}