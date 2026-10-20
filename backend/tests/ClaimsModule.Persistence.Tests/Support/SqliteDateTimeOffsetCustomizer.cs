using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Tests.Support;

internal sealed class SqliteDateTimeOffsetCustomizer(ModelCustomizerDependencies dependencies)
    : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        var converter = new DateTimeOffsetToBinaryConverter();
        var properties = modelBuilder.Model.GetEntityTypes()
            .Where(entityType => entityType.GetViewName() is null)
            .SelectMany(entityType => entityType.GetProperties()
                .Concat(entityType.GetComplexProperties().SelectMany(complex => complex.ComplexType.GetProperties())))
            .Where(property => (Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType)
                == typeof(DateTimeOffset))
            .ToList();

        foreach (var property in properties)
        {
            property.SetValueConverter(converter);
        }
    }
}
