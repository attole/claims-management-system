using ClaimsModule.Domain.Claims;
using ClaimsModule.Persistence.Converters;

namespace ClaimsModule.Persistence.Tests.Converters;

public sealed class ClosedSetConverterTests
{
    private readonly ClosedSetConverter<ClaimState, ClaimStateKind> _converter = new(ClaimState.From);

    [Fact]
    public void ConvertToProvider_Variant_ReturnsTheMarkerText()
    {
        var result = _converter.ConvertToProvider(new ClaimUnderInvestigationState());

        result.Should().Be("UnderInvestigation");
    }

    [Fact]
    public void ConvertFromProvider_MarkerText_ReturnsTheVariant()
    {
        var result = _converter.ConvertFromProvider("PendingPayment");

        result.Should().Be(new ClaimPendingPaymentState());
    }

    [Fact]
    public void ConvertFromProvider_UnknownText_Throws()
    {
        Func<object?> act = () => _converter.ConvertFromProvider("Archived");

        act.Should().Throw<ArgumentException>();
    }
}
