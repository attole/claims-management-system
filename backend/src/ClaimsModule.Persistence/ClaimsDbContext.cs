using ClaimsModule.Application.Abstractions;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Configurations;
using ClaimsModule.Persistence.Rows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence;

public sealed class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options, ICurrentUser currentUser)
    : DbContext(options)
{
    private const string SequentialIdSql = "NEWSEQUENTIALID()";

    public Guid CurrentOrganisationId => currentUser.OrganisationId;

    public DbSet<Claim> Claims => Set<Claim>();

    public DbSet<Policy> Policies => Set<Policy>();

    public DbSet<CauseOfLossCode> CauseOfLossCodes => Set<CauseOfLossCode>();

    public DbSet<ReserveComponent> ReserveComponents => Set<ReserveComponent>();

    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();

    internal DbSet<PolicyCoverageType> PolicyCoverageTypes => Set<PolicyCoverageType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ClaimConfiguration(this));
        modelBuilder.ApplyConfiguration(new LossEventConfiguration(this));
        modelBuilder.ApplyConfiguration(new ClaimPartyConfiguration(this));
        modelBuilder.ApplyConfiguration(new ClaimRiskObjectConfiguration(this));
        modelBuilder.ApplyConfiguration(new ClaimValidationIssueConfiguration(this));
        modelBuilder.ApplyConfiguration(new ClaimStateChangeConfiguration(this));
        modelBuilder.ApplyConfiguration(new ClaimListRowConfiguration(this));
        modelBuilder.ApplyConfiguration(new ReserveComponentConfiguration(this));
        modelBuilder.ApplyConfiguration(new ReserveHistoryConfiguration(this));
        modelBuilder.ApplyConfiguration(new ClaimDocumentConfiguration(this));
        modelBuilder.ApplyConfiguration(new PolicyConfiguration(this));
        modelBuilder.ApplyConfiguration(new PolicyCoverageTypeConfiguration(this));
        modelBuilder.ApplyConfiguration(new CauseOfLossCodeConfiguration(this));

        ConfigureMoneyPrecision(modelBuilder);

        var isSqlServer = Database.IsSqlServer();
        var tables = modelBuilder.Model.GetEntityTypes()
            .Where(entityType => typeof(Entity).IsAssignableFrom(entityType.ClrType))
            .ToList();

        foreach (var table in tables)
        {
            var builder = modelBuilder.Entity(table.ClrType);

            ConfigureKey(builder, isSqlServer);
            AddAuditColumns(builder);

            if (typeof(ISoftDeletable).IsAssignableFrom(table.ClrType))
            {
                AddSoftDeleteColumns(builder);
            }

            if (typeof(AggregateRoot).IsAssignableFrom(table.ClrType))
            {
                AddRowVersion(builder, isSqlServer);
            }
        }
    }

    private static void ConfigureMoneyPrecision(ModelBuilder modelBuilder)
    {
        var moneyProperties = modelBuilder.Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetProperties()
                .Concat(entityType.GetComplexProperties().SelectMany(complex => complex.ComplexType.GetProperties())))
            .Where(property => property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
            .ToList();

        foreach (var property in moneyProperties)
        {
            property.SetPrecision(19);
            property.SetScale(4);
        }
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
