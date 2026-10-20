using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Conventions;

internal static class EntityTableConvention
{
    private const string SequentialIdSql = "NEWSEQUENTIALID()";

    public static void ConfigureEntityTables(ModelBuilder modelBuilder, bool isSqlServer)
    {
        foreach (var table in FindEntityTables(modelBuilder))
        {
            var builder = modelBuilder.Entity(table.ClrType);

            ConfigureKey(builder, isSqlServer);
            AddAuditColumns(builder);

            if (HasSoftDelete(table))
            {
                AddSoftDeleteColumns(builder);
            }

            if (typeof(AggregateRoot).IsAssignableFrom(table.ClrType))
            {
                AddRowVersion(builder, isSqlServer);
            }
        }
    }

    public static IReadOnlyList<IMutableEntityType> FindEntityTables(ModelBuilder modelBuilder)
    {
        var tables = modelBuilder.Model.GetEntityTypes()
            .Where(entityType => typeof(Entity).IsAssignableFrom(entityType.ClrType))
            .ToList();

        return tables;
    }

    public static bool HasSoftDelete(IReadOnlyEntityType entityType)
    {
        return typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType);
    }

    private static void ConfigureKey(EntityTypeBuilder builder, bool isSqlServer)
    {
        var key = builder.Property(nameof(Entity.Id)).ValueGeneratedNever();

        if (isSqlServer)
        {
            key.HasDefaultValueSql(SequentialIdSql);
        }
    }

    private static void AddAuditColumns(EntityTypeBuilder builder)
    {
        builder.Property<DateTimeOffset>(RowColumns.CreatedAt);
        builder.Property<DateTimeOffset?>(RowColumns.UpdatedAt);
        builder.Property<Guid?>(RowColumns.UserCreated);
        builder.Property<Guid?>(RowColumns.UserModified);
    }

    private static void AddRowVersion(EntityTypeBuilder builder, bool isSqlServer)
    {
        var rowVersion = builder.Property(nameof(AggregateRoot.RowVer)).IsConcurrencyToken();

        if (isSqlServer)
        {
            rowVersion.IsRowVersion();
        }
    }

    private static void AddSoftDeleteColumns(EntityTypeBuilder builder)
    {
        builder.Property(RowColumns.IsDeleted).HasDefaultValue(false);
        builder.Property<DateTimeOffset?>(RowColumns.DeletedAt);
    }
}
