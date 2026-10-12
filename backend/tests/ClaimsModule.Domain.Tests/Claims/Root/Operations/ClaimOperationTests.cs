using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Operations;

public sealed class ClaimOperationTests
{
    [Fact]
    public void AssignHandler_SetsTheHandler()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant());
        Guid handler = Guid.NewGuid();

        new AssignHandlerOperation().Execute(claim, new AssignHandlerInput(handler, TestClaims.Later));

        claim.AssignedHandlerId.Should().Be(handler);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void SetSeverity_SetsTheSeverity()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant());

        new SetSeverityOperation().Execute(
            claim,
            new SetSeverityInput(ClaimSeverity.Critical, TestClaims.Later));

        claim.Severity.Should().Be(ClaimSeverity.Critical);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void UpdateNotes_ReplacesTheNotes()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant());

        new UpdateNotesOperation().Execute(claim, new UpdateNotesInput("called the insured", TestClaims.Later));

        claim.Notes.Should().Be("called the insured");
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }
}
