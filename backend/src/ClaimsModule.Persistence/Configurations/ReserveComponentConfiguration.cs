using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Conventions;
using ClaimsModule.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ReserveComponentConfiguration : IEntityTypeConfiguration<ReserveComponent>
{
    public void Configure(EntityTypeBuilder<ReserveComponent> builder)
    {
        builder.ToTable("ClaimReserveComponents");
        builder.HasKey(component => component.Id);
        builder.Property(component => component.Id).HasColumnName("ReserveComponentId");
        builder.Property(component => component.ComponentType)
            .HasColumnName("Component")
            .HasConversion(new ClosedSetConverter<ReserveComponentType, ReserveComponentTypeKind>(
                ReserveComponentType.From))
            .HasMaxLength(50);
        builder.Property(component => component.State)
            .HasColumnName("Status")
            .HasConversion(new ClosedSetConverter<ReserveComponentState, ReserveComponentStateKind>(
                ReserveComponentState.From))
            .HasMaxLength(50);
        builder.HasMany(component => component.History)
            .WithOne()
            .HasForeignKey(history => history.ReserveComponentId)
            .MarksRootModified();
        builder.Navigation(component => component.History).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(component => component.ClaimId);
    }
}
