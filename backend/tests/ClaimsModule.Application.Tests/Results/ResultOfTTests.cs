using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Tests.Results;

public sealed class ResultOfTTests
{
    [Fact]
    public void Success_HoldsTheValueAndTheWarnings()
    {
        var warning = new Warning("CODE", "message");

        var result = Result<string>.Success("value", [warning]);

        result.Value.Should().Be("value");
        result.Warnings.Should().Equal(warning);
    }

    [Fact]
    public void Failure_HoldsTheErrorsAndNoWarnings()
    {
        var error = Error.Conflict("CODE", "message");

        var result = Result<string>.Failure([error]);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Equal(error);
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Value_OfAFailure_Throws()
    {
        var result = Result<string>.Failure([Error.NotFound("CODE", "message")]);

        Func<string> act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Map_OfASuccess_MapsTheValueAndKeepsTheWarnings()
    {
        var warning = new Warning("CODE", "message");
        var success = Result<string>.Success("value", [warning]);

        var result = success.Map(value => value.Length);

        result.Value.Should().Be(5);
        result.Warnings.Should().Equal(warning);
    }

    [Fact]
    public void Map_OfAFailure_KeepsTheErrors()
    {
        var error = Error.Conflict("CODE", "message");
        var failure = Result<string>.Failure([error]);

        var result = failure.Map(value => value.Length);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Equal(error);
    }

    [Fact]
    public void Failure_WithNoErrors_Throws()
    {
        Func<Result<string>> act = () => Result<string>.Failure([]);

        act.Should().Throw<ArgumentException>();
    }
}
