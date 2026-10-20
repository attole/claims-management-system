using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal abstract class SoftDeleteConfiguration<TEntity>(ClaimsDbContext context) : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity, ISoftDeletable
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasQueryFilter(entity => entity.OrganisationId == context.CurrentOrganisationId
            && !EF.Property<bool>(entity, RowColumns.IsDeleted));
    }
}
