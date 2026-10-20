using ClaimsModule.Domain.Common.Capabilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal abstract class TenantConfiguration<TEntity>(ClaimsDbContext context) : IEntityTypeConfiguration<TEntity>
    where TEntity : class, ITenantOwned
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasQueryFilter(entity => entity.OrganisationId == context.CurrentOrganisationId);
    }
}
