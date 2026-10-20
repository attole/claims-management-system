using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Persistence.Converters;

namespace ClaimsModule.Persistence.Tests.Converters;

public sealed class EstimatedLossAmountConverterTests
{
    private readonly EstimatedLossAmountConverter _converter = new();

    [Fact]
    public void ConvertToProvider_EmptyEstimate_ReturnsNull()
    {
        var result = _converter.ConvertToProvider(EstimatedLossAmount.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public void ConvertToProvider_ZeroEstimate_ReturnsZero()
    {
        var result = _converter.ConvertToProvider(new EstimatedLossAmount(0m));

        result.Should().Be(0m);
    }

    [Fact]
    public void ConvertFromProvider_Null_ReturnsEmpty()
    {
        var result = _converter.ConvertFromProvider(null);

        result.Should().Be(EstimatedLossAmount.Empty);
    }

    [Fact]
    public void ConvertFromProvider_StoredZero_ReturnsASetEstimate()
    {
        var result = _converter.ConvertFromProvider(0m);

        result.Should().Be(new EstimatedLossAmount(0m));
    }
}
