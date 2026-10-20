using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ReserveHistoryConfiguration(ClaimsDbContext context)
    : TenantConfiguration<ReserveHistory>(context)
{
    public override void Configure(EntityTypeBuilder<ReserveHistory> builder)
    {
        base.Configure(builder);

        builder.ToTable("ReserveHistory");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).HasColumnName("ReserveHistoryId");
        builder.ComplexProperty(history => history.Details, details =>
        {
            details.Property(value => value.ClaimId).HasColumnName("ClaimId");
            details.Property(value => value.ComponentType)
                .HasColumnName("ComponentType")
                .HasConversion(new ClosedSetConverter<ReserveComponentType, ReserveComponentTypeKind>(
                    ReserveComponentType.From))
                .HasMaxLength(50);
            details.Property(value => value.TransactionType)
                .HasColumnName("TransactionType")
                .HasConversion<string>()
                .HasMaxLength(50);
            details.Property(value => value.Amount).HasColumnName("Amount");
            details.Property(value => value.ChangeReason)
                .HasColumnName("ChangeReason")
                .HasMaxLength(ReserveFieldLengths.ChangeReason);
            details.Property(value => value.ChangeSequence).HasColumnName("ChangeSequence");
        });
        builder.Property(history => history.ApprovalState)
            .HasColumnName("ApprovalStatus")
            .HasConversion(new ClosedSetConverter<ReserveApprovalState, ReserveApprovalStateKind>(
                ReserveApprovalState.From))
            .HasMaxLength(50);
        builder.Property(history => history.PostingState)
            .HasColumnName("PostingStatus")
            .HasConversion(new ClosedSetConverter<ReservePostingState, ReservePostingStateKind>(
                ReservePostingState.From))
            .HasMaxLength(50);
        builder.ComplexProperty(history => history.PostingDetails, posting =>
        {
            posting.Property(value => value.JobId).HasColumnName("PostingJobId").HasMaxLength(100);
            posting.Property(value => value.FailureReason).HasColumnName("PostingFailureReason").HasMaxLength(500);
        });
        builder.ComplexProperty(history => history.Balances, balances =>
        {
            balances.Property(value => value.PreviousBalance).HasColumnName("PreviousBalance");
            balances.Property(value => value.NewBalance).HasColumnName("NewBalance");
            balances.Property(value => value.IsSet).HasColumnName("HasBalances");
        });
        builder.ComplexProperty(history => history.Review, review =>
        {
            review.Property(value => value.ReviewedBy).HasColumnName("ReviewedBy");
            review.Property(value => value.ReviewedAt).HasColumnName("ReviewedAt");
            review.Property(value => value.Reason)
                .HasColumnName("ReviewReason")
                .HasMaxLength(ReserveFieldLengths.ReviewReason);
        });
    }
}
