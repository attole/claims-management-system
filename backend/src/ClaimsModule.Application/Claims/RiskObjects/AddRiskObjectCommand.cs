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

public sealed record AddRiskObjectCommand(Guid ClaimId, RiskObjectRequest RiskObject) : ICommand<Result<RiskObjectDto>>
{
    public AddRiskObjectInput ToInput(PolicyPeriod period, CauseCodes codes, DateTimeOffset now)
    {
        return new AddRiskObjectInput(RiskObject.ToDetails(), period, codes, now);
    }
}

public sealed class AddRiskObjectCommandValidator : AbstractValidator<AddRiskObjectCommand>
{
    public AddRiskObjectCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.RiskObject).NotNull().SetValidator(new RiskObjectRequestValidator());
    }
}

public sealed class AddRiskObjectCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IClock clock,
    IMapper mapper) : IRequestHandler<AddRiskObjectCommand, Result<RiskObjectDto>>
{
    public async Task<Result<RiskObjectDto>> Handle(AddRiskObjectCommand command, CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<RiskObjectDto>();
        }

        var claim = loaded.Value;
        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var outcome = claim.AddRiskObject(command.ToInput(period, codes, clock.UtcNow));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<RiskObjectDto>(value));
    }
}
