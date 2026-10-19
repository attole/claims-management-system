using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Tests.Claims;

public sealed class ClaimErrorsTests
{
    [Fact]
    public void ClaimNotFound_IsANotFoundErrorWithItsCode()
    {
        var error = ClaimErrors.ClaimNotFound(Guid.NewGuid());

        error.Kind.Should().Be(ErrorKind.NotFound);
        error.Code.Should().Be(ClaimErrors.ClaimNotFoundCode);
    }

    [Theory]
    [InlineData(ClaimErrors.PartyNotFoundCode)]
    [InlineData(ClaimErrors.RiskObjectNotFoundCode)]
    [InlineData(ClaimErrors.IssueNotFoundCode)]
    [InlineData(ClaimErrors.PolicyNotFoundCode)]
    public void Factories_AreNotFoundErrorsWithDistinctCodes(string code)
    {
        var errors = new[]
        {
            ClaimErrors.PartyNotFound(Guid.NewGuid()),
            ClaimErrors.RiskObjectNotFound(Guid.NewGuid()),
            ClaimErrors.IssueNotFound(Guid.NewGuid()),
            ClaimErrors.PolicyNotFound(Guid.NewGuid()),
        };

        errors.Should().ContainSingle(error => error.Code == code);
        errors.Should().OnlyContain(error => error.Kind == ErrorKind.NotFound);
    }
}
