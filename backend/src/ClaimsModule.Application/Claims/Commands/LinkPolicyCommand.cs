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

namespace ClaimsModule.Application.Claims.Commands;

public sealed record LinkPolicyCommand(Guid ClaimId, Guid PolicyId) : ICommand<Result<ClaimDto>>
{
    public LinkPolicyInput ToInput(PolicyPeriod period, CauseCodes codes)
    {
        return new LinkPolicyInput(PolicyId, period, codes);
    }
}

public sealed class LinkPolicyCommandValidator : AbstractValidator<LinkPolicyCommand>
{
    public LinkPolicyCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.PolicyId).NotEmpty();
    }
}

public sealed class LinkPolicyCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<LinkPolicyCommand, Result<ClaimDto>>
{
    public async Task<Result<ClaimDto>> Handle(LinkPolicyCommand command, CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ClaimDto>();
        }

        var claim = loaded.Value;

        bool hasPolicy = await policies.ExistsAsync(command.PolicyId, cancellationToken);

        if (!hasPolicy)
        {
            return Result<ClaimDto>.Failure([ClaimErrors.PolicyNotFound(command.PolicyId)]);
        }

        var period = await policies.GetPeriodAsync(command.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var outcome = claim.LinkPolicy(command.ToInput(period, codes));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<ClaimDto>(value));
    }
}
