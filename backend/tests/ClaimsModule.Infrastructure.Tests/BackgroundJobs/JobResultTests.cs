using ClaimsModule.Application.Results;
using ClaimsModule.Infrastructure.BackgroundJobs;

namespace ClaimsModule.Infrastructure.Tests.BackgroundJobs;

public sealed class JobResultTests
{
    [Fact]
    public void ThrowIfFailed_ForASuccess_DoesNotThrow()
    {
        var act = () => JobResult.ThrowIfFailed(Result.Success());

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfFailed_ForAFailure_ThrowsNamingTheErrorCodes()
    {
        var result = Result.Failure([Error.NotFound("FIRST", "one"), Error.Conflict("SECOND", "two")]);

        var act = () => JobResult.ThrowIfFailed(result);

        act.Should().Throw<InvalidOperationException>().WithMessage("*FIRST, SECOND*");
    }
}
