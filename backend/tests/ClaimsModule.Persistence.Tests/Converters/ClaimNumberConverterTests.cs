using ClaimsModule.Domain.Claims;
using ClaimsModule.Persistence.Converters;

namespace ClaimsModule.Persistence.Tests.Converters;

public sealed class ClaimNumberConverterTests
{
    [Fact]
    public void ConvertToProvider_ClaimNumber_IsItsText()
    {
        var converter = new ClaimNumberConverter();

        var result = converter.ConvertToProvider(ClaimNumber.Create(2026, 42));

        result.Should().Be("CLM-2026-0000042");
    }

    [Fact]
    public void ConvertFromProvider_Text_IsTheClaimNumber()
    {
        var converter = new ClaimNumberConverter();

        var result = converter.ConvertFromProvider("CLM-2026-0000042");

        result.Should().Be(ClaimNumber.Create(2026, 42));
    }

    [Fact]
    public void ConvertFromProvider_TextThatIsNotAClaimNumber_Throws()
    {
        var converter = new ClaimNumberConverter();

        Func<object?> act = () => converter.ConvertFromProvider("not a number");

        act.Should().Throw<ArgumentException>();
    }
}
