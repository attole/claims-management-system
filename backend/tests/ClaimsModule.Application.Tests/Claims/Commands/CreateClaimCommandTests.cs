using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Commands;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Commands;

public sealed class CreateClaimCommandTests
{
    [Fact]
    [Trait("Rule", "S:INTAKE-SingleCall")]
    public async Task Handle_WithChildren_CreatesTheClaimInOneUnitOfWork()
    {
        var context = new ClaimHandlerContext();
        var handler = CreateHandler(context);
        var command = CreateCommand(
            Guid.Empty,
            [ApplicationRequests.Claimant(), ApplicationRequests.Witness()],
            [ApplicationRequests.Kitchen()]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClaimNumber.Should().Be("CLM-2026-0000001");
        result.Value.State.Should().Be("Draft");
        context.VerifyAdded(Times.Once());
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    [Trait("Rule", "S:INTAKE-NoPolicy")]
    public async Task Handle_WithoutAPolicy_StoresThePolicyUnknownIssue()
    {
        var context = new ClaimHandlerContext();
        var handler = CreateHandler(context);
        var command = CreateCommand(Guid.Empty, [ApplicationRequests.Claimant()], [ApplicationRequests.Kitchen()]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var issues = context.AddedClaims.Single().Issues;

        issues.Select(issue => issue.Finding.Code).Should().Contain("S:INTAKE-NoPolicy");
    }

    [Fact]
    public async Task Handle_WithAPolicy_LinksTheClaimToIt()
    {
        var period = new PolicyPeriod(
            Guid.NewGuid(),
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            DateOnly.MinValue);
        var context = new ClaimHandlerContext().WithPolicy(period);
        var handler = CreateHandler(context);
        var command = CreateCommand(period.PolicyId, [ApplicationRequests.Claimant()], [ApplicationRequests.Kitchen()]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.PolicyId.Should().Be(period.PolicyId);
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenThePolicyDoesNotExist_ReturnsNotFoundAndStoresNothing()
    {
        var policyId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutPolicy(policyId);
        var handler = CreateHandler(context);
        var command = CreateCommand(policyId, [ApplicationRequests.Claimant()], [ApplicationRequests.Kitchen()]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.PolicyNotFound(policyId));
        context.VerifyAdded(Times.Never());
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenAChildStepIsRefused_ReturnsTheErrorsAndStoresNothing()
    {
        var context = new ClaimHandlerContext();
        var handler = CreateHandler(context);
        var primaryKitchen = ApplicationRequests.Kitchen() with { IsPrimary = true };
        var primaryGarage = ApplicationRequests.Garage() with { IsPrimary = true };
        var command = CreateCommand(Guid.Empty, [ApplicationRequests.Claimant()], [primaryKitchen, primaryGarage]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        context.VerifyAdded(Times.Never());
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new CreateClaimCommandValidator();
        var command = CreateValidatorCommand([ApplicationRequests.Claimant()]);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithoutAReportDate_FailsOnTheField()
    {
        var validator = new CreateClaimCommandValidator();
        var command = CreateValidatorCommand([ApplicationRequests.Claimant()]) with { ReportDate = default };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ReportDate);
    }

    [Fact]
    public void Validate_WithAnInvalidParty_FailsOnThePartyField()
    {
        var validator = new CreateClaimCommandValidator();
        var party = ApplicationRequests.Claimant() with { Role = (PartyRoleKind)99 };
        var command = CreateValidatorCommand([party]);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Parties[0].Role");
    }

    [Fact]
    [Trait("Rule", "S:API-POST-claims")]
    public async Task Handle_WithAnInitialReserve_StoresTheComponentWithTheClaim()
    {
        var period = new PolicyPeriod(
            Guid.NewGuid(),
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            DateOnly.MinValue);
        var context = new ClaimHandlerContext().WithPolicy(period);
        var handler = CreateHandler(context);
        var reserve = new ReserveRequest(ReserveTransactionType.Add, 5000m, "Initial estimate")
        {
            ComponentType = ReserveComponentTypeKind.Indemnity,
        };
        var command = CreateCommand(period.PolicyId, [ApplicationRequests.Claimant()], [ApplicationRequests.Kitchen()])
            with { Reserves = [reserve] };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        context.AddedComponents.Single().History.Single().Details.Amount.Should().Be(5000m);
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    [Trait("Rule", "S:API-POST-claims")]
    public async Task Handle_WhenTheInitialReserveIsRefused_StoresNothing()
    {
        var context = new ClaimHandlerContext();
        var handler = CreateHandler(context);
        var reserve = new ReserveRequest(ReserveTransactionType.Add, 5000m, "Initial estimate")
        {
            ComponentType = ReserveComponentTypeKind.Indemnity,
        };
        var command = CreateCommand(Guid.Empty, [ApplicationRequests.Claimant()], [ApplicationRequests.Kitchen()])
            with { Reserves = [reserve] };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        context.VerifyAdded(Times.Never());
        context.AddedComponents.Should().BeEmpty();
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_WithAReserveForAnExistingComponent_FailsOnTheReserve()
    {
        var validator = new CreateClaimCommandValidator();
        var reserve = new ReserveRequest(ReserveTransactionType.Add, 5000m, "Initial estimate")
        {
            ComponentId = Guid.NewGuid(),
        };
        var command = CreateValidatorCommand([ApplicationRequests.Claimant()]) with { Reserves = [reserve] };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Reserves[0]");
    }

    private static CreateClaimCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new CreateClaimCommandHandler(
            context.Claims,
            context.Numbers,
            context.Policies,
            context.ReserveComponents,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.User,
            context.Mapper);
    }

    private static CreateClaimCommand CreateCommand(
        Guid policyId,
        IReadOnlyList<PartyRequest> parties,
        IReadOnlyList<RiskObjectRequest> riskObjects)
    {
        var reportDate = new DateTimeOffset(2026, 10, 10, 8, 0, 0, TimeSpan.Zero);

        return new CreateClaimCommand(policyId, reportDate, ApplicationRequests.Loss(), parties, riskObjects);
    }

    private static CreateClaimCommand CreateValidatorCommand(IReadOnlyList<PartyRequest> parties)
    {
        var reportDate = new DateTimeOffset(2026, 10, 10, 8, 0, 0, TimeSpan.Zero);

        return new CreateClaimCommand(
            Guid.Empty,
            reportDate,
            ApplicationRequests.Loss(),
            parties,
            [ApplicationRequests.Kitchen()]);
    }
}
