using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands;

public sealed record SetLimitOverrideCommand(Guid ClaimId, bool HasAggregateLimitOverride)
    : ICommand<Result<ClaimDto>>
{
    public SetLimitOverrideInput ToInput(
        PolicyPeriod period,
        CauseCodes codes,
        UserRole role,
        decimal approvedTotal,
        DateTimeOffset now)
    {
        return new SetLimitOverrideInput(HasAggregateLimitOverride, role, approvedTotal, period, codes, now);
    }
}

public sealed class SetLimitOverrideCommandValidator : AbstractValidator<SetLimitOverrideCommand>
{
    public SetLimitOverrideCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
    }
}

public sealed class SetLimitOverrideCommandHandler(
    IClaimRepository claims,
    IClaimReserveReader reserves,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user,
    IMapper mapper) : IRequestHandler<SetLimitOverrideCommand, Result<ClaimDto>>
{
    public async Task<Result<ClaimDto>> Handle(SetLimitOverrideCommand command, CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ClaimDto>();
        }

        var claim = loaded.Value;
        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var facts = await reserves.GetFactsAsync(claim.Id, cancellationToken);
        var input = command.ToInput(period, codes, user.Role, facts.ApprovedTotal, clock.UtcNow);
        var result = claim.SetLimitOverride(input).ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<ClaimDto>(value));
    }
}
