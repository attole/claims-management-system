using ClaimsModule.Application.Claims;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimPartyConfiguration : IEntityTypeConfiguration<ClaimParty>
{
    public void Configure(EntityTypeBuilder<ClaimParty> builder)
    {
        builder.ToTable("ClaimParties");
        builder.HasKey(party => party.Id);
        builder.Property(party => party.Id).HasColumnName("ClaimPartyId");
        builder.Property(party => party.IsActive);
        builder.ComplexProperty(party => party.Details, details =>
        {
            details.Property(value => value.Role)
                .HasColumnName("PartyRole")
                .HasConversion(new ClosedSetConverter<PartyRole, PartyRoleKind>(PartyRole.From))
                .HasMaxLength(50);
            details.Property(value => value.Type)
                .HasColumnName("PartyType")
                .HasConversion(new ClosedSetConverter<PartyType, PartyTypeKind>(PartyType.From))
                .HasMaxLength(20);
            details.Property(value => value.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(ClaimFieldLengths.FirstName);
            details.Property(value => value.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(ClaimFieldLengths.LastName);
            details.Property(value => value.CompanyName)
                .HasColumnName("CompanyName")
                .HasMaxLength(ClaimFieldLengths.CompanyName);
            details.Property(value => value.Email).HasColumnName("Email").HasMaxLength(ClaimFieldLengths.Email);
            details.Property(value => value.Phone).HasColumnName("Phone").HasMaxLength(ClaimFieldLengths.Phone);
            details.Property(value => value.Notes).HasColumnName("Notes");
        });
        builder.HasIndex(party => party.ClaimId);
    }
}
