using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Tests.Support.Interceptors;

internal sealed class ConventionContext(DbContextOptions<ConventionContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var appendOnly = modelBuilder.Entity<AppendOnlyRow>();
        appendOnly.HasKey(row => row.Id);
        AddAuditColumns(appendOnly);

        var softDeletable = modelBuilder.Entity<SoftDeletableRow>();
        softDeletable.HasKey(row => row.Id);
        AddAuditColumns(softDeletable);
        softDeletable.Property(RowColumns.IsDeleted).HasDefaultValue(false);
        softDeletable.Property<DateTimeOffset?>(RowColumns.DeletedAt);
    }

    private static void AddAuditColumns(EntityTypeBuilder builder)
    {
        builder.Property<DateTimeOffset>(RowColumns.CreatedAt);
        builder.Property<DateTimeOffset?>(RowColumns.UpdatedAt);
        builder.Property<Guid?>(RowColumns.UserCreated);
        builder.Property<Guid?>(RowColumns.UserModified);
    }
}
