using ClaimsModule.Persistence.Rows;
using ClaimsModule.Persistence.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

internal sealed class ClaimListRowConfiguration(ClaimsDbContext context)
    : TenantConfiguration<ClaimListRow>(context)
{
    public override void Configure(EntityTypeBuilder<ClaimListRow> builder)
    {
        base.Configure(builder);

        builder.HasNoKey();
        builder.ToView(ClaimListView.Name);
        builder.Property(row => row.LossDate).IsRequired(false);
    }
}
