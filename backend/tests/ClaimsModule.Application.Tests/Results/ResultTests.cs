using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Tests.Results;

public sealed class ResultTests
{
    [Fact]
    public void Success_HasNoErrors()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_KeepsTheWarnings()
    {
        var warning = new Warning("CODE", "message");

        var result = Result.Success([warning]);

        result.Warnings.Should().Equal(warning);
    }

    [Fact]
    public void Failure_HoldsTheErrors()
    {
        var error = Error.NotFound("CODE", "message");

        var result = Result.Failure([error]);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Equal(error);
    }

    [Fact]
    public void Failure_WithNoErrors_Throws()
    {
        Func<Result> act = () => Result.Failure([]);

        act.Should().Throw<ArgumentException>();
    }
}
