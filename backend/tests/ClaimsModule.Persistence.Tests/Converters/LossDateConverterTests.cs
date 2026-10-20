using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Persistence.Converters;

namespace ClaimsModule.Persistence.Tests.Converters;

public sealed class LossDateConverterTests
{
    private readonly LossDateConverter _converter = new();

    [Fact]
    public void ConvertToProvider_EmptyLossDate_ReturnsNull()
    {
        var result = _converter.ConvertToProvider(LossDate.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public void ConvertToProvider_SetLossDate_ReturnsTheTime()
    {
        var time = new DateTimeOffset(2026, 2, 3, 10, 0, 0, TimeSpan.Zero);

        var result = _converter.ConvertToProvider(new LossDate(time));

        result.Should().Be(time);
    }

    [Fact]
    public void ConvertFromProvider_Null_ReturnsEmpty()
    {
        var result = _converter.ConvertFromProvider(null);

        result.Should().Be(LossDate.Empty);
    }

    [Fact]
    public void ConvertFromProvider_Time_ReturnsSetLossDate()
    {
        var time = new DateTimeOffset(2026, 2, 3, 10, 0, 0, TimeSpan.Zero);

        var result = _converter.ConvertFromProvider(time);

        result.Should().Be(new LossDate(time));
    }
}
