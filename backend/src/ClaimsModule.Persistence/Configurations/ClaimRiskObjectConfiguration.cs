using ClaimsModule.Application.Claims;
using ClaimsModule.Domain.Claims.RiskObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimRiskObjectConfiguration : IEntityTypeConfiguration<ClaimRiskObject>
{
    public void Configure(EntityTypeBuilder<ClaimRiskObject> builder)
    {
        builder.ToTable("ClaimRiskObjects");
        builder.HasKey(riskObject => riskObject.Id);
        builder.Property(riskObject => riskObject.Id).HasColumnName("ClaimRiskObjectId");
        builder.ComplexProperty(riskObject => riskObject.Details, details =>
        {
            details.Property(value => value.AssetType)
                .HasColumnName("AssetType")
                .HasConversion<string>()
                .HasMaxLength(50);
            details.Property(value => value.AssetDescription)
                .HasColumnName("AssetDescription")
                .HasMaxLength(ClaimFieldLengths.AssetDescription);
            details.Property(value => value.DamageDescription).HasColumnName("DamageDescription");
            details.Property(value => value.IsPrimary).HasColumnName("IsPrimary");
            details.Property(value => value.AssetReference)
                .HasColumnName("AssetReference")
                .HasMaxLength(ClaimFieldLengths.AssetReference);
        });
        builder.HasIndex(riskObject => riskObject.ClaimId);
    }
}
