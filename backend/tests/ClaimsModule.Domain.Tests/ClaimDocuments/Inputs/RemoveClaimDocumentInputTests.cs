using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Domain.Tests.ClaimDocuments.Inputs;

public sealed class RemoveClaimDocumentInputTests
{
    [Fact]
    public void Now_WhenNotGiven_Throws()
    {
        Func<RemoveClaimDocumentInput> act = () => new RemoveClaimDocumentInput(new ClaimOpenState(), default);

        act.Should().Throw<ArgumentException>();
    }
}
