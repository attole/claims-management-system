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
            return Result<ClaimStatusDto>.Failure([ClaimErrors.StatusUnsupported(command.TargetStatus.ToString())]);
        }

        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var facts = await reserves.GetFactsAsync(claim.Id, cancellationToken);
        var context = new ClaimStatusContext(command, period, codes, facts, user.Role);
        var result = run(claim, context).ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(move => new ClaimStatusDto(claim.Id, move.From.Name, claim.State.Name));
    }
}
