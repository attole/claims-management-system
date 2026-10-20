using ClaimsModule.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Support.Conventions;

internal sealed class ConventionContext(DbContextOptions<ConventionContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppendOnlyRow>().HasKey(row => row.Id);
        modelBuilder.Entity<SoftDeletableRow>().HasKey(row => row.Id);
        EntityTableConvention.ConfigureEntityTables(modelBuilder, false);
    }
}
