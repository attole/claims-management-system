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

public sealed record UpdateRiskObjectCommand(Guid ClaimId, Guid RiskObjectId, RiskObjectRequest RiskObject)
    : ICommand<Result<RiskObjectDto>>
{
    public UpdateRiskObjectInput ToInput(PolicyPeriod period, CauseCodes codes)
    {
        return new UpdateRiskObjectInput(RiskObjectId, RiskObject.ToDetails(), period, codes);
    }
}

public sealed class UpdateRiskObjectCommandValidator : AbstractValidator<UpdateRiskObjectCommand>
{
    public UpdateRiskObjectCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.RiskObjectId).NotEmpty();
        RuleFor(command => command.RiskObject).NotNull().SetValidator(new RiskObjectRequestValidator());
    }
}

public sealed class UpdateRiskObjectCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateRiskObjectCommand, Result<RiskObjectDto>>
{
    public async Task<Result<RiskObjectDto>> Handle(
        UpdateRiskObjectCommand command,
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
        var outcome = claim.UpdateRiskObject(command.ToInput(period, codes));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<RiskObjectDto>(value));
    }
}
