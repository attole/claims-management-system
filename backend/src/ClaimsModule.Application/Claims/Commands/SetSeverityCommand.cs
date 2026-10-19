using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Policies;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands;

public sealed record SetSeverityCommand(Guid ClaimId, ClaimSeverity Severity) : ICommand<Result<ClaimDto>>
{
    public SetSeverityInput ToInput(PolicyPeriod period, CauseCodes codes)
    {
        return new SetSeverityInput(Severity, period, codes);
    }
}

public sealed class SetSeverityCommandValidator : AbstractValidator<SetSeverityCommand>
{
    public SetSeverityCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.Severity).IsInEnum();
    }
}

public sealed class SetSeverityCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<SetSeverityCommand, Result<ClaimDto>>
{
    public async Task<Result<ClaimDto>> Handle(SetSeverityCommand command, CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ClaimDto>();
        }

        var claim = loaded.Value;
        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var outcome = claim.SetSeverity(command.ToInput(period, codes));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<ClaimDto>(value));
    }
}
