using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimValidationIssueConfiguration : IEntityTypeConfiguration<ClaimValidationIssue>
{
    public void Configure(EntityTypeBuilder<ClaimValidationIssue> builder)
    {
        builder.ToTable("ClaimIssues");
        builder.HasKey(issue => issue.Id);
        builder.Property(issue => issue.Id).HasColumnName("ClaimIssueId");
        builder.Property(issue => issue.State)
            .HasColumnName("State")
            .HasConversion(new ClosedSetConverter<IssueState, IssueStateKind>(IssueState.From))
            .HasMaxLength(50);
        builder.Property(issue => issue.ResolvedAt);
        builder.Property(issue => issue.ResolvedBy);
        builder.ComplexProperty(issue => issue.Finding, finding =>
        {
            finding.Property(value => value.Code).HasColumnName("Code").HasMaxLength(100);
            finding.Property(value => value.Message).HasColumnName("Message").HasMaxLength(500);
            finding.Property(value => value.Field).HasColumnName("Field").HasMaxLength(100);
            finding.Property(value => value.Severity)
                .HasColumnName("Severity")
                .HasConversion<string>()
                .HasMaxLength(50);
            finding.Property(value => value.Evidence).HasColumnName("Evidence");
        });
        builder.HasIndex(issue => issue.ClaimId);
    }
}
