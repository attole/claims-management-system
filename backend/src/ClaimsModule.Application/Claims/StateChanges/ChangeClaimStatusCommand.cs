using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
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
        RuleFor(command => command.ReasonCode).MaximumLength(ClaimFieldLengths.ReasonCode);
        RuleFor(command => command.ReasonText).MaximumLength(ClaimFieldLengths.ReasonText);
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

        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var facts = await reserves.GetFactsAsync(claim.Id, cancellationToken);
        var context = new ClaimStatusContext(command, period, codes, facts, user.Role, clock.UtcNow);
        var result = ClaimStatusRunner.Run(claim, context);

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(move => new ClaimStatusDto(claim.Id, move.From.Name, claim.State.Name));
    }
}
