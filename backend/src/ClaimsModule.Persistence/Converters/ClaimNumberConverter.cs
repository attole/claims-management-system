using System.Linq.Expressions;
using ClaimsModule.Domain.Claims;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClaimsModule.Persistence.Converters;

internal sealed class ClaimNumberConverter() : ValueConverter<ClaimNumber, string>(_toColumn, _fromColumn)
{
    private static readonly Expression<Func<ClaimNumber, string>> _toColumn = number => number.Value;

    private static readonly Expression<Func<string, ClaimNumber>> _fromColumn = text => ClaimNumber.From(text);
}
