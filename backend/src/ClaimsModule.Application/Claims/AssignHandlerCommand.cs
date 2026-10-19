using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Policies;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims;

public sealed record AssignHandlerCommand(Guid ClaimId, Guid HandlerId) : ICommand<Result<ClaimDto>>
{
    public AssignHandlerInput ToInput(PolicyPeriod period, CauseCodes codes)
    {
        return new AssignHandlerInput(HandlerId, period, codes);
    }
}

public sealed class AssignHandlerCommandValidator : AbstractValidator<AssignHandlerCommand>
{
    public AssignHandlerCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.HandlerId).NotEmpty();
    }
}

public sealed class AssignHandlerCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<AssignHandlerCommand, Result<ClaimDto>>
{
    public async Task<Result<ClaimDto>> Handle(AssignHandlerCommand command, CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ClaimDto>();
        }

        var claim = loaded.Value;
        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var outcome = claim.AssignHandler(command.ToInput(period, codes));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<ClaimDto>(value));
    }
}
