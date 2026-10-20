using ClaimsModule.Application.Claims;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class LossEventConfiguration : IEntityTypeConfiguration<LossEvent>
{
    public void Configure(EntityTypeBuilder<LossEvent> builder)
    {
        builder.ToTable("LossEvents");
        builder.HasKey(loss => loss.Id);
        builder.Property(loss => loss.Id).HasColumnName("LossEventId");
        builder.Property(loss => loss.ReportDate);
        builder.ComplexProperty(loss => loss.Details, details =>
        {
            details.Property(value => value.LossDate)
                .HasColumnName("LossDate")
                .HasConversion(new LossDateConverter())
                .IsRequired(false);
            details.Property(value => value.Description).HasColumnName("LossDescription");
            details.Property(value => value.Location)
                .HasColumnName("LossLocation")
                .HasMaxLength(ClaimFieldLengths.Location);
            details.Property(value => value.CauseOfLossCode)
                .HasColumnName("CauseOfLossCode")
                .HasMaxLength(ClaimFieldLengths.CauseOfLossCode);
            details.Property(value => value.EstimatedLossAmount)
                .HasColumnName("EstimatedLossAmount")
                .HasConversion(new EstimatedLossAmountConverter())
                .HasPrecision(19, 4)
                .IsRequired(false);
            details.Property(value => value.PoliceReportNumber)
                .HasColumnName("PoliceReportNumber")
                .HasMaxLength(ClaimFieldLengths.PoliceReportNumber);
        });
    }
}
