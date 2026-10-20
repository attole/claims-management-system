using ClaimsModule.Application.Claims;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimStateChangeConfiguration(ClaimsDbContext context)
    : TenantConfiguration<ClaimStateChange>(context)
{
    public override void Configure(EntityTypeBuilder<ClaimStateChange> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClaimStateChanges");
        builder.HasKey(stateChange => stateChange.Id);
        builder.Property(stateChange => stateChange.Id).HasColumnName("ClaimStateChangeId");
        builder.Property(stateChange => stateChange.From)
            .HasColumnName("FromStatus")
            .HasConversion(new ClosedSetConverter<ClaimState, ClaimStateKind>(ClaimState.From))
            .HasMaxLength(50);
        builder.Property(stateChange => stateChange.To)
            .HasColumnName("ToStatus")
            .HasConversion(new ClosedSetConverter<ClaimState, ClaimStateKind>(ClaimState.From))
            .HasMaxLength(50);
        builder.Property(stateChange => stateChange.Note);
        builder.ComplexProperty(stateChange => stateChange.Reason, reason =>
        {
            reason.Property(value => value.Code).HasColumnName("ReasonCode").HasMaxLength(ClaimFieldLengths.ReasonCode);
            reason.Property(value => value.Text).HasColumnName("ReasonText").HasMaxLength(ClaimFieldLengths.ReasonText);
        });
        builder.HasIndex(stateChange => stateChange.ClaimId);
    }
}
