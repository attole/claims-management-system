using System.Reflection;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.Claims;
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
    public Guid CurrentOrganisationId => currentUser.OrganisationId;

    public DbSet<Claim> Claims => Set<Claim>();

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

    private void ApplyFilters(ModelBuilder modelBuilder)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        foreach (var table in EntityTableConvention.FindEntityTables(modelBuilder))
        {
            if (EntityTableConvention.HasSoftDelete(table))
            {
                var method = typeof(ClaimsDbContext).GetMethod(nameof(ApplyTenantAndSoftDeleteFilter), flags)!;
                method.MakeGenericMethod(table.ClrType).Invoke(this, [modelBuilder]);
            }
            else
            {
                var method = typeof(ClaimsDbContext).GetMethod(nameof(ApplyTenantFilter), flags)!;
                method.MakeGenericMethod(table.ClrType).Invoke(this, [modelBuilder]);
            }
        }

        modelBuilder.Entity<ClaimListRow>().HasQueryFilter(row => row.OrganisationId == CurrentOrganisationId);
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
