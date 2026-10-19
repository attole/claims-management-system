using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims.Loss;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands;

public sealed record CreateClaimCommand(
    Guid PolicyId,
    DateTimeOffset ReportDate,
    LossRequest Loss,
    IReadOnlyList<PartyRequest> Parties,
    IReadOnlyList<RiskObjectRequest> RiskObjects) : ICommand<Result<ClaimDto>>
{
    public IReadOnlyList<ReserveRequest> Reserves { get; init; } = [];

    public CreateClaimInput ToInput(Guid organisationId, ClaimNumber claimNumber, DateTimeOffset now, CauseCodes codes)
    {
        return new CreateClaimInput(organisationId, claimNumber, ReportDate, now, PolicyPeriod.Empty, codes);
    }
}

public sealed class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    private const string NewComponentRequiredMessage = "A new claim has no component to add to; give a component type.";

    public CreateClaimCommandValidator()
    {
        RuleFor(command => command.ReportDate).NotEmpty();
        RuleFor(command => command.Loss).NotNull().SetValidator(new LossRequestValidator());
        RuleFor(command => command.Parties).NotNull();
        RuleForEach(command => command.Parties).SetValidator(new PartyRequestValidator());
        RuleFor(command => command.RiskObjects).NotNull();
        RuleForEach(command => command.RiskObjects).SetValidator(new RiskObjectRequestValidator());
        RuleFor(command => command.Reserves).NotNull();
        RuleForEach(command => command.Reserves).SetValidator(new ReserveRequestValidator());
        RuleForEach(command => command.Reserves)
            .Must(reserve => reserve.IsNewComponent)
            .WithMessage(NewComponentRequiredMessage);
    }
}

public sealed class CreateClaimCommandHandler(
    IClaimRepository claims,
    IClaimNumberGenerator numbers,
    IPolicyReader policies,
    IReserveComponentRepository components,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user,
    IMapper mapper) : IRequestHandler<CreateClaimCommand, Result<ClaimDto>>
{
    public async Task<Result<ClaimDto>> Handle(CreateClaimCommand command, CancellationToken cancellationToken)
    {
        bool isPolicyKnown = await IsPolicyKnownAsync(command, cancellationToken);

        if (!isPolicyKnown)
        {
            return Result<ClaimDto>.Failure([ClaimErrors.PolicyNotFound(command.PolicyId)]);
        }

        var now = clock.UtcNow;
        var number = await numbers.NextAsync(now.Year, cancellationToken);
        var period = await policies.GetPeriodAsync(command.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var created = Claim.Create(command.ToInput(user.OrganisationId, number, now, codes)).ToResult();

        if (!created.IsSuccess)
        {
            return created.ToFailure<ClaimDto>();
        }

        var assembled = Assemble(created.Value, command, period, codes);

        if (!assembled.IsSuccess)
        {
            return assembled.ToFailure<ClaimDto>();
        }

        var opened = new List<ReserveComponent>();
        var reserved = OpenReserves(assembled.Value, command, opened, user.Id, now);

        if (!reserved.IsSuccess)
        {
            return reserved.ToFailure<ClaimDto>();
        }

        await claims.AddAsync(assembled.Value, cancellationToken);

        foreach (var component in opened)
        {
            await components.AddAsync(component, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        var warnings = created.Warnings
            .Concat(assembled.Warnings)
            .Concat(reserved.Warnings)
            .ToList();

        return Result<ClaimDto>.Success(mapper.Map<ClaimDto>(assembled.Value), warnings);
    }

    private static Result<Claim> Assemble(
        Claim claim,
        CreateClaimCommand command,
        PolicyPeriod period,
        CauseCodes codes)
    {
        var warnings = new List<Warning>();

        foreach (var step in Populate(claim, command, period, codes))
        {
            if (!step.IsSuccess)
            {
                return step.ToFailure<Claim>();
            }

            warnings.AddRange(step.Warnings);
        }

        return Result<Claim>.Success(claim, warnings);
    }

    private static Result OpenReserves(
        Claim claim,
        CreateClaimCommand command,
        List<ReserveComponent> opened,
        Guid actorId,
        DateTimeOffset now)
    {
        var warnings = new List<Warning>();
        decimal approvedTotal = 0m;

        foreach (var reserve in command.Reserves)
        {
            var component = ReserveComponent.Open(reserve.ToOpenInput(claim.Reference)).ToResult();

            if (!component.IsSuccess)
            {
                return component;
            }

            var input = reserve.ToSubmitInput(claim, approvedTotal, actorId, now);
            var submitted = component.Value.SubmitHistory(input).ToResult();

            if (!submitted.IsSuccess)
            {
                return submitted;
            }

            opened.Add(component.Value);
            warnings.AddRange(submitted.Warnings);
            approvedTotal += ApprovedAmount(submitted.Value);
        }

        return Result.Success(warnings);
    }

    private static decimal ApprovedAmount(ReserveHistory history)
    {
        return history.ApprovalState.IsApproved ? history.Details.Amount : 0m;
    }

    private static IEnumerable<Result> Populate(
        Claim claim,
        CreateClaimCommand command,
        PolicyPeriod period,
        CauseCodes codes)
    {
        if (command.PolicyId != Guid.Empty)
        {
            var policyInput = new LinkPolicyInput(command.PolicyId, period, codes);

            yield return claim.LinkPolicy(policyInput).ToResult();
        }

        var lossInput = new UpdateLossDetailsInput(command.Loss.ToDetails(), period, codes);

        yield return claim.UpdateLossDetails(lossInput).ToResult();

        foreach (var riskObject in command.RiskObjects)
        {
            var riskObjectInput = new AddRiskObjectInput(riskObject.ToDetails(), period, codes);

            yield return claim.AddRiskObject(riskObjectInput).ToResult();
        }

        foreach (var party in command.Parties)
        {
            var partyInput = new AddPartyInput(party.ToDetails(), period, codes);

            yield return claim.AddParty(partyInput).ToResult();
        }
    }

    private async Task<bool> IsPolicyKnownAsync(CreateClaimCommand command, CancellationToken cancellationToken)
    {
        bool hasNoPolicy = command.PolicyId == Guid.Empty;

        return hasNoPolicy || await policies.ExistsAsync(command.PolicyId, cancellationToken);
    }
}
