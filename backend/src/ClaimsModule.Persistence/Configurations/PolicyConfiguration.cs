using ClaimsModule.Domain.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");
        builder.HasKey(policy => policy.Id);
        builder.Property(policy => policy.Id).HasColumnName("PolicyId");
        builder.Property(policy => policy.PolicyNumber).HasMaxLength(50).IsRequired();
        builder.Property(policy => policy.ClientName).HasMaxLength(255).IsRequired();
        builder.Property<DateOnly>("EffectiveDate");
        builder.Property<DateOnly>("ExpirationDate");
        builder.Property<DateOnly?>("CancelledDate");
        builder.Ignore(policy => policy.Period);
        builder.Ignore(policy => policy.CoverageTypes);
        builder.HasIndex(policy => policy.PolicyNumber).IsUnique();
    }
}
