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

    public DbSet<MotorFloodClaim> MotorFloodClaims
        => Set<MotorFloodClaim>();

    public DbSet<MotorProduct> MotorProducts
        => Set<MotorProduct>();

    public DbSet<ClaimHistory> ClaimHistories
        => Set<ClaimHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InsurancePolicy>()
            .HasIndex(policy => policy.PolicyNumber)
            .IsUnique();

        modelBuilder.Entity<MotorFloodClaim>()
            .HasIndex(claim => claim.ClaimNumber)
            .IsUnique();

        modelBuilder.Entity<MotorFloodClaim>()
            .HasOne(claim => claim.InsurancePolicy)
            .WithMany()
            .HasForeignKey(claim => claim.InsurancePolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MotorProduct>()
            .HasIndex(product => product.Code)
            .IsUnique();

        modelBuilder.Entity<InsurancePolicy>()
            .HasOne(policy => policy.MotorProduct)
            .WithMany()
            .HasForeignKey(policy => policy.MotorProductId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ClaimHistory>()
            .HasIndex(claim => claim.ClaimNumber)
            .IsUnique();

        modelBuilder.Entity<ClaimHistory>()
            .HasOne(claim => claim.InsurancePolicy)
            .WithMany(policy => policy.ClaimHistories)
            .HasForeignKey(claim => claim.InsurancePolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
