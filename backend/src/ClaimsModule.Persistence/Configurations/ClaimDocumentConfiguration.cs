using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimDocumentConfiguration(ClaimsDbContext context)
    : SoftDeleteConfiguration<ClaimDocument>(context)
{
    public override void Configure(EntityTypeBuilder<ClaimDocument> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClaimDocuments");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.Id).HasColumnName("ClaimDocumentId");
        builder.ComplexProperty(document => document.Details, details =>
        {
            details.Property(value => value.DocumentType)
                .HasColumnName("DocumentType")
                .HasMaxLength(DocumentFieldLengths.DocumentType);
            details.Property(value => value.DocumentName)
                .HasColumnName("DocumentName")
                .HasMaxLength(DocumentFieldLengths.DocumentName);
            details.Property(value => value.BlobPath)
                .HasColumnName("BlobPath")
                .HasMaxLength(DocumentFieldLengths.BlobPath);
            details.Property(value => value.ContentType)
                .HasColumnName("ContentType")
                .HasMaxLength(DocumentFieldLengths.ContentType);
            details.Property(value => value.FileSizeBytes).HasColumnName("FileSizeBytes");
            details.Property(value => value.Notes).HasColumnName("Notes").HasMaxLength(DocumentFieldLengths.Notes);
        });
        builder.HasOne<Claim>().WithMany().HasForeignKey(document => document.ClaimId).MarksRootModified();
        builder.HasIndex(document => document.ClaimId);
    }
}
