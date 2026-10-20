using System.Linq.Expressions;
using ClaimsModule.Domain.Claims.Loss;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Converters;

internal sealed class EstimatedLossAmountConverter()
    : ValueConverter<EstimatedLossAmount, decimal?>(_toColumn, _fromColumn)
{
    private static readonly Expression<Func<EstimatedLossAmount, decimal?>> _toColumn =
        amount => amount.IsSet ? amount.Value : null;

    private static readonly Expression<Func<decimal?, EstimatedLossAmount>> _fromColumn =
        value => value.HasValue ? new EstimatedLossAmount(value.Value) : EstimatedLossAmount.Empty;

    public override bool ConvertsNulls => true;
}
