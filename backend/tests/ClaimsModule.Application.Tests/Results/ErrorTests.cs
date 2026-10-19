using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Tests.Results;

public sealed class ErrorTests
{
    [Fact]
    public void Validation_KeepsTheFieldAndTheKind()
    {
        var error = Error.Validation("CODE", "message", "Field");

        error.Should().Be(new Error("CODE", "message", ErrorKind.Validation, "Field"));
    }

    [Theory]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Forbidden)]
    [InlineData(ErrorKind.Conflict)]
    public void Factories_HaveNoFieldAndTheirOwnKind(ErrorKind kind)
    {
        var error = CreateError(kind);

        error.Kind.Should().Be(kind);
        error.Field.Should().BeEmpty();
    }

    private static Error CreateError(ErrorKind kind)
    {
        return kind switch
        {
            ErrorKind.NotFound => Error.NotFound("CODE", "message"),
            ErrorKind.Forbidden => Error.Forbidden("CODE", "message"),
            _ => Error.Conflict("CODE", "message"),
        };
    }
}
