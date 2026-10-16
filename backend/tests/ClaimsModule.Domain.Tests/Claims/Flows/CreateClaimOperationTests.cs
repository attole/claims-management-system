using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class CreateClaimOperationTests
{
    [Fact]
    public void Run_StartsAsDraftWithUnspecifiedSeverity()
    {
        var input = CreateInput();

        var outcome = CreateClaimOperation.Run(input);

        outcome.Value.State.Should().BeOfType<ClaimDraftState>();
        outcome.Value.Severity.Should().Be(ClaimSeverity.Unspecified);
        outcome.Value.ReportedDate.Should().Be(TestClaims.Start);
        outcome.Value.OrganisationId.Should().Be(TestClaims.Organisation);
        outcome.Value.ClaimNumber.Value.Should().Be("CLM-2026-0000001");
    }

    [Fact]
    public void Run_StartsWithoutPartiesRiskObjectsOrLossDetails()
    {
        var input = CreateInput();

        var outcome = CreateClaimOperation.Run(input);

        outcome.Value.Parties.Should().BeEmpty();
        outcome.Value.RiskObjects.Should().BeEmpty();
        outcome.Value.LossEvent.ClaimId.Should().Be(outcome.Value.Id);
        outcome.Value.LossEvent.Details.Should().Be(LossDetails.Empty);
    }

    [Fact]
    [Trait("Rule", HasOrganisation.Code)]
    public void Run_WithoutOrganisation_Fails()
    {
        var number = ClaimNumber.From("CLM-2026-0000001");
        var input = new CreateClaimInput(
            Guid.Empty,
            number,
            TestClaims.Start,
            TestClaims.Start,
            TestClaims.LossReference());

        var outcome = CreateClaimOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasOrganisation.Code);
    }

    [Fact]
    [Trait("Rule", HasClaimNumber.Code)]
    public void Run_WithoutAClaimNumber_Fails()
    {
        var organisation = TestClaims.Organisation;
        var input = new CreateClaimInput(
            organisation,
            ClaimNumber.Empty,
            TestClaims.Start,
            TestClaims.Start,
            TestClaims.LossReference());

        var outcome = CreateClaimOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasClaimNumber.Code);
    }

    [Fact]
    [Trait("Rule", HasReportDate.Code)]
    public void Run_WithoutAReportDate_Fails()
    {
        var number = ClaimNumber.From("CLM-2026-0000001");
        var input = new CreateClaimInput(
            TestClaims.Organisation,
            number,
            DateTimeOffset.MinValue,
            TestClaims.Start,
            TestClaims.LossReference());

        var outcome = CreateClaimOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasReportDate.Code);
    }

    [Fact]
    [Trait("Rule", IsReportDateNotInFuture.Code)]
    public void Run_WithAReportDateAfterTheCreationTime_Fails()
    {
        var number = ClaimNumber.From("CLM-2026-0000001");
        var input = new CreateClaimInput(
            TestClaims.Organisation,
            number,
            TestClaims.Later,
            TestClaims.Start,
            TestClaims.LossReference());

        var outcome = CreateClaimOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReportDateNotInFuture.Code);
    }

    [Fact]
    public void Run_StoresTheGivenReportDateOnTheLossAndTheTimeAsReceived()
    {
        var reportDate = TestClaims.Start.AddDays(-2);
        var number = ClaimNumber.From("CLM-2026-0000001");
        var input = new CreateClaimInput(
            TestClaims.Organisation,
            number,
            reportDate,
            TestClaims.Start,
            TestClaims.LossReference());

        var outcome = CreateClaimOperation.Run(input);

        outcome.Value.LossEvent.ReportDate.Should().Be(reportDate);
        outcome.Value.ReportedDate.Should().Be(TestClaims.Start);
    }

    private static CreateClaimInput CreateInput()
    {
        var number = ClaimNumber.From("CLM-2026-0000001");

        var reference = TestClaims.LossReference();

        return new CreateClaimInput(TestClaims.Organisation, number, TestClaims.Start, TestClaims.Start, reference);
    }
}
