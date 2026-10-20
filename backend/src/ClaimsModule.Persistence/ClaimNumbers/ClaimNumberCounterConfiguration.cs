using ClaimsModule.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.ClaimNumbers;

internal sealed class ClaimNumberCounterConfiguration(ClaimsDbContext context)
    : TenantConfiguration<ClaimNumberCounter>(context)
{
    public override void Configure(EntityTypeBuilder<ClaimNumberCounter> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClaimNumberCounters");
        builder.HasKey(counter => new { counter.OrganisationId, counter.Year });
    }
}
