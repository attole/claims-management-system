using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Claims.StateChanges;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries;

public sealed record CheckClaimStatusQuery(Guid ClaimId, ClaimStateKind TargetStatus)
    : IQuery<Result<ClaimStatusCheckDto>>;

public sealed class CheckClaimStatusQueryValidator : AbstractValidator<CheckClaimStatusQuery>
{
    public CheckClaimStatusQueryValidator()
    {
        RuleFor(query => query.ClaimId).NotEmpty();
        RuleFor(query => query.TargetStatus).IsInEnum();
    }
}

public sealed class CheckClaimStatusQueryHandler(
    IClaimRepository claims,
    IClaimReserveReader reserves,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IClock clock,
    ICurrentUser user) : IRequestHandler<CheckClaimStatusQuery, Result<ClaimStatusCheckDto>>
{
    public async Task<Result<ClaimStatusCheckDto>> Handle(
        CheckClaimStatusQuery query,
        CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(query.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ClaimStatusCheckDto>();
        }

        var claim = loaded.Value;
        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var facts = await reserves.GetFactsAsync(claim.Id, cancellationToken);
        var command = new ChangeClaimStatusCommand(claim.Id, query.TargetStatus);
        var context = new ClaimStatusContext(command, period, codes, facts, user.Role, clock.UtcNow);
        var result = ClaimStatusRunner.Run(claim, context);

        return Result<ClaimStatusCheckDto>.Success(new ClaimStatusCheckDto(result.Errors, result.Warnings));
    }
}
