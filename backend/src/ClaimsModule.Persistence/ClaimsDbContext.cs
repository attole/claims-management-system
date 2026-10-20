using System.Reflection;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Persistence.Conventions;
using ClaimsModule.Persistence.Rows;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence;

public sealed class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options, ICurrentUser currentUser)
    : DbContext(options)
{
    private static readonly MethodInfo _tenantFilterMethod = FindMethod(nameof(ApplyTenantFilter));

    private static readonly MethodInfo _softDeleteFilterMethod = FindMethod(nameof(ApplyTenantAndSoftDeleteFilter));

    public Guid CurrentOrganisationId => currentUser.OrganisationId;

    public DbSet<Policy> Policies => Set<Policy>();

    public DbSet<CauseOfLossCode> CauseOfLossCodes => Set<CauseOfLossCode>();

    public DbSet<ReserveComponent> ReserveComponents => Set<ReserveComponent>();

    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();

    internal DbSet<PolicyCoverageType> PolicyCoverageTypes => Set<PolicyCoverageType>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(19, 4);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
        EntityTableConvention.ConfigureEntityTables(modelBuilder, Database.IsSqlServer());
        ApplyFilters(modelBuilder);
    }

    private static MethodInfo FindMethod(string name)
    {
        return typeof(ClaimsDbContext).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
    }

    private void ApplyFilters(ModelBuilder modelBuilder)
    {
        var tables = EntityTableConvention.FindEntityTables(modelBuilder);
        var withSoftDelete = tables.Where(table => EntityTableConvention.HasSoftDelete(table));
        var withoutSoftDelete = tables.Where(table => !EntityTableConvention.HasSoftDelete(table));

        foreach (var table in withSoftDelete)
        {
            _softDeleteFilterMethod.MakeGenericMethod(table.ClrType).Invoke(this, [modelBuilder]);
        }

        foreach (var table in withoutSoftDelete)
        {
            _tenantFilterMethod.MakeGenericMethod(table.ClrType).Invoke(this, [modelBuilder]);
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : Entity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => entity.OrganisationId == CurrentOrganisationId);
    }

    private void ApplyTenantAndSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : Entity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => entity.OrganisationId == CurrentOrganisationId
            && !EF.Property<bool>(entity, RowColumns.IsDeleted));
    }
}
