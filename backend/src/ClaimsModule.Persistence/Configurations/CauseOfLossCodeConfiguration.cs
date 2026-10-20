using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Persistence.Schema.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class CauseOfLossCodeConfiguration(ClaimsDbContext context)
    : SoftDeleteConfiguration<CauseOfLossCode>(context)
{
    public override void Configure(EntityTypeBuilder<CauseOfLossCode> builder)
    {
        base.Configure(builder);

        builder.ToTable("CauseOfLossCodes");
        builder.HasKey(code => code.Id);
        builder.Property(code => code.Id).HasColumnName("CauseOfLossCodeId");
        builder.Property(code => code.Code).HasMaxLength(50).IsRequired();
        builder.Property(code => code.Name).HasMaxLength(255).IsRequired();
        builder.Property(code => code.PerilCategory).HasConversion<string>().HasMaxLength(50);
        builder.Property(code => code.IsActive).HasDefaultValue(false);
        builder.Property<int>("SortOrder");
        builder.HasIndex(code => code.Code).IsUnique();
        builder.HasData(CauseOfLossSeed.Codes());
    }
}
