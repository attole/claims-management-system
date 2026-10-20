using System.Collections.Frozen;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Flows;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.StateChanges;

public sealed record ChangeClaimStatusCommand(Guid ClaimId, ClaimStateKind TargetStatus)
    : ICommand<Result<ClaimStatusDto>>
{
    public string ReasonCode { get; init; } = string.Empty;

    public string ReasonText { get; init; } = string.Empty;

    public string Note { get; init; } = string.Empty;

    public bool IsLiabilityConfirmed { get; init; } = false;
}

public sealed class ChangeClaimStatusCommandValidator : AbstractValidator<ChangeClaimStatusCommand>
{
    public ChangeClaimStatusCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.TargetStatus).IsInEnum();
    }
}

public sealed class ChangeClaimStatusCommandHandler(
    IClaimRepository claims,
    IClaimReserveReader reserves,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user) : IRequestHandler<ChangeClaimStatusCommand, Result<ClaimStatusDto>>
{
    private static readonly FrozenDictionary<ClaimStateKind, Func<Claim, ClaimStatusContext, Outcome<Move<ClaimState>>>>
        _runners = new Dictionary<ClaimStateKind, Func<Claim, ClaimStatusContext, Outcome<Move<ClaimState>>>>
        {
            [ClaimStateKind.Open] = (claim, context) => claim.Open(context.ToTransitionInput()),
            [ClaimStateKind.UnderInvestigation] = (claim, context) => claim.Investigate(context.ToTransitionInput()),
            [ClaimStateKind.PendingPayment] = (claim, context) => claim.SendToPayment(context.ToSendToPaymentInput()),
            [ClaimStateKind.Closed] = (claim, context) => claim.Close(context.ToFreezeInput()),
            [ClaimStateKind.Withdrawn] = (claim, context) => claim.Withdraw(context.ToFreezeInput()),
            [ClaimStateKind.Reopened] = (claim, context) => claim.Reopen(context.ToReopenInput()),
        }.ToFrozenDictionary();

    public async Task<Result<ClaimStatusDto>> Handle(
        ChangeClaimStatusCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ClaimStatusDto>();
        }

        var claim = loaded.Value;

        if (!_runners.TryGetValue(command.TargetStatus, out var run))
        {
            var unsupported = ClaimErrors.StatusUnsupported(command.TargetStatus.ToString(), NextStatusNames(claim));

            return Result<ClaimStatusDto>.Failure([unsupported]);
        }

        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var facts = await reserves.GetFactsAsync(claim.Id, cancellationToken);
        var context = new ClaimStatusContext(command, period, codes, facts, user.Role, clock.UtcNow);
        var result = ExplainRefusal(claim, run(claim, context).ToResult());

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(move => new ClaimStatusDto(claim.Id, move.From.Name, claim.State.Name));
    }

    private static Result<Move<ClaimState>> ExplainRefusal(Claim claim, Result<Move<ClaimState>> result)
    {
        var refusal = result.Errors.FirstOrDefault(error => error.Code == TransitionCodes.NotPermitted);

        if (refusal is null)
        {
            return result;
        }

        var explained = ClaimErrors.WithNextStatuses(refusal, NextStatusNames(claim));

        return Result<Move<ClaimState>>.Failure([explained]);
    }

    private static IReadOnlyList<string> NextStatusNames(Claim claim)
    {
        return ClaimTransitions.NextFrom(claim.State)
            .Select(state => state.Name)
            .ToList();
    }
}
