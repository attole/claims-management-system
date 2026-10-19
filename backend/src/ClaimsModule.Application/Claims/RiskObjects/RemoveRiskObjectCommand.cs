using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Policies;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.RiskObjects;

public sealed record RemoveRiskObjectCommand(Guid ClaimId, Guid RiskObjectId) : ICommand<Result<RiskObjectDto>>
{
    public RemoveRiskObjectInput ToInput(PolicyPeriod period, CauseCodes codes, DateTimeOffset now)
    {
        return new RemoveRiskObjectInput(RiskObjectId, now, period, codes);
    }
}

public sealed class RemoveRiskObjectCommandValidator : AbstractValidator<RemoveRiskObjectCommand>
{
    public RemoveRiskObjectCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.RiskObjectId).NotEmpty();
    }
}

public sealed class RemoveRiskObjectCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IClock clock,
    IMapper mapper) : IRequestHandler<RemoveRiskObjectCommand, Result<RiskObjectDto>>
{
    public async Task<Result<RiskObjectDto>> Handle(
        RemoveRiskObjectCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<RiskObjectDto>();
        }

        var claim = loaded.Value;

        bool hasRiskObject = claim.RiskObjects.Any(riskObject => riskObject.Id == command.RiskObjectId);

        if (!hasRiskObject)
        {
            return Result<RiskObjectDto>.Failure([ClaimErrors.RiskObjectNotFound(command.RiskObjectId)]);
        }

        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var outcome = claim.RemoveRiskObject(command.ToInput(period, codes, clock.UtcNow));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<RiskObjectDto>(value));
    }
}
