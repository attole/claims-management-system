using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Policies;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Loss;

public sealed record UpdateLossDetailsCommand(Guid ClaimId, LossRequest Loss) : ICommand<Result<LossDetailsDto>>
{
    public UpdateLossDetailsInput ToInput(PolicyPeriod period, CauseCodes codes)
    {
        return new UpdateLossDetailsInput(Loss.ToDetails(), period, codes);
    }
}

public sealed class UpdateLossDetailsCommandValidator : AbstractValidator<UpdateLossDetailsCommand>
{
    public UpdateLossDetailsCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.Loss).NotNull().SetValidator(new LossRequestValidator());
    }
}

public sealed class UpdateLossDetailsCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateLossDetailsCommand, Result<LossDetailsDto>>
{
    public async Task<Result<LossDetailsDto>> Handle(
        UpdateLossDetailsCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<LossDetailsDto>();
        }

        var claim = loaded.Value;
        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var outcome = claim.UpdateLossDetails(command.ToInput(period, codes));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<LossDetailsDto>(value));
    }
}
