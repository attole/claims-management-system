using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimConfiguration(ClaimsDbContext context)
    : SoftDeleteConfiguration<Claim>(context)
{
    public override void Configure(EntityTypeBuilder<Claim> builder)
    {
        base.Configure(builder);

        builder.ToTable("Claims");
        builder.HasKey(claim => claim.Id);
        builder.Property(claim => claim.Id).HasColumnName("ClaimId");
        builder.Property(claim => claim.ClaimNumber)
            .HasConversion(new ClaimNumberConverter())
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(claim => claim.State)
            .HasColumnName("Status")
            .HasConversion(new ClosedSetConverter<ClaimState, ClaimStateKind>(ClaimState.From))
            .HasMaxLength(50);
        builder.Property(claim => claim.Severity).HasConversion<string>().HasMaxLength(50);
        builder.Property(claim => claim.ReportedDate);
        builder.Property(claim => claim.PolicyId);
        builder.Property(claim => claim.AssignedHandlerId);
        builder.Property(claim => claim.Notes);
        builder.Property(claim => claim.HasAggregateLimitOverride);
        builder.HasOne(claim => claim.LossEvent)
            .WithOne()
            .HasForeignKey<LossEvent>(loss => loss.ClaimId)
            .IsRequired()
            .MarksRootModified();
        builder.HasMany(claim => claim.Parties)
            .WithOne()
            .HasForeignKey(party => party.ClaimId)
            .MarksRootModified();
        builder.HasMany(claim => claim.RiskObjects)
            .WithOne()
            .HasForeignKey(riskObject => riskObject.ClaimId)
            .MarksRootModified();
        builder.HasMany(claim => claim.Issues)
            .WithOne()
            .HasForeignKey(issue => issue.ClaimId)
            .MarksRootModified();
        builder.HasMany(claim => claim.StateChanges)
            .WithOne()
            .HasForeignKey(stateChange => stateChange.ClaimId)
            .OnDelete(DeleteBehavior.Restrict)
            .MarksRootModified();
        builder.Navigation(claim => claim.Parties).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(claim => claim.RiskObjects).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(claim => claim.Issues).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(claim => claim.StateChanges).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(claim => new { claim.OrganisationId, claim.ClaimNumber }).IsUnique();
        builder.HasIndex(claim => claim.State);
        builder.HasIndex(claim => claim.PolicyId);
    }
}
