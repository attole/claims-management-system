using ClaimsModule.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal abstract class TenantConfiguration<TEntity>(ClaimsDbContext context) : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasQueryFilter(entity => entity.OrganisationId == context.CurrentOrganisationId);
    }
}
