using ClaimsModule.Domain.Claims;
using ClaimsModule.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Audit;

internal sealed class AuditLogEntryConfiguration(ClaimsDbContext context) : TenantConfiguration<AuditLogEntry>(context)
{
    private const int NameLength = 100;

    public override void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClaimAuditLog");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).HasColumnName("AuditLogId");
        builder.Property(entry => entry.EventType).HasMaxLength(NameLength).IsRequired();
        builder.Property(entry => entry.Description).IsRequired();
        builder.Property(entry => entry.OldValue);
        builder.Property(entry => entry.NewValue);
        builder.Property(entry => entry.RelatedEntityId);
        builder.Property(entry => entry.RelatedEntityType).HasMaxLength(NameLength);
        builder.Property(entry => entry.CorrelationId);
        builder.HasIndex(entry => new { entry.ClaimId, entry.EventType });
        builder.HasOne<Claim>().WithMany().HasForeignKey(entry => entry.ClaimId).OnDelete(DeleteBehavior.Restrict);
    }
}
