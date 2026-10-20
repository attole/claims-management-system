using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimDocumentConfiguration : IEntityTypeConfiguration<ClaimDocument>
{
    public void Configure(EntityTypeBuilder<ClaimDocument> builder)
    {
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
        builder.HasIndex(document => document.ClaimId);
    }
}
