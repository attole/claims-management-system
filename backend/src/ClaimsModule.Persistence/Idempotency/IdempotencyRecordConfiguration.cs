using ClaimsModule.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Idempotency;

internal sealed class IdempotencyRecordConfiguration(ClaimsDbContext context)
    : TenantConfiguration<IdempotencyRecord>(context)
{
    private const int KeyLength = 200;
    private const int MethodLength = 10;
    private const int RouteLength = 500;
    private const int BodyHashLength = 128;

    public override void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        base.Configure(builder);

        builder.ToTable("IdempotencyKeys");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).HasColumnName("IdempotencyKeyId").ValueGeneratedNever();
        builder.Property(record => record.Key).HasMaxLength(KeyLength).IsRequired();
        builder.Property(record => record.Method).HasMaxLength(MethodLength).IsRequired();
        builder.Property(record => record.Route).HasMaxLength(RouteLength).IsRequired();
        builder.Property(record => record.BodyHash).HasMaxLength(BodyHashLength).IsRequired();
        builder.Property(record => record.ResponseBody).IsRequired();
        builder.Property(record => record.CreatedAt).IsConcurrencyToken();
        builder.HasIndex(record => new
        {
            record.Key,
            record.OrganisationId,
            record.UserId,
            record.Method,
            record.Route,
        }).IsUnique();
    }
}
