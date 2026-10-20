using ClaimsModule.Domain.Policies;
using ClaimsModule.Persistence.Rows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class PolicyCoverageTypeConfiguration : IEntityTypeConfiguration<PolicyCoverageType>
{
    public void Configure(EntityTypeBuilder<PolicyCoverageType> builder)
    {
        builder.ToTable("PolicyCoverageTypes");
        builder.HasKey(coverageType => coverageType.Id);
        builder.Property(coverageType => coverageType.Id).HasColumnName("PolicyCoverageTypeId");
        builder.Property(coverageType => coverageType.Name).HasMaxLength(255).IsRequired();
        builder.HasIndex(coverageType => new { coverageType.PolicyId, coverageType.Name }).IsUnique();
        builder.HasOne<Policy>().WithMany().HasForeignKey(coverageType => coverageType.PolicyId);
    }
}
