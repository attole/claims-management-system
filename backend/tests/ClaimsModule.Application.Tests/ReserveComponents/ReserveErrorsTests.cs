using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Tests.ReserveComponents;

public sealed class ReserveErrorsTests
{
    [Fact]
    public void ComponentNotFound_IsANotFoundErrorWithItsCode()
    {
        var error = ReserveErrors.ComponentNotFound(Guid.NewGuid());

        error.Kind.Should().Be(ErrorKind.NotFound);
        error.Code.Should().Be(ReserveErrors.ComponentNotFoundCode);
    }

    [Fact]
    public void HistoryNotFound_IsANotFoundErrorWithItsCode()
    {
        var error = ReserveErrors.HistoryNotFound(Guid.NewGuid());

        error.Kind.Should().Be(ErrorKind.NotFound);
        error.Code.Should().Be(ReserveErrors.HistoryNotFoundCode);
    }
}
