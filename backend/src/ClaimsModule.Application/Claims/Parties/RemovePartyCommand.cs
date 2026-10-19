using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Policies;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Parties;

public sealed record RemovePartyCommand(Guid ClaimId, Guid PartyId) : ICommand<Result<PartyDto>>
{
    public RemovePartyInput ToInput(PolicyPeriod period, CauseCodes codes)
    {
        return new RemovePartyInput(PartyId, period, codes);
    }
}

public sealed class RemovePartyCommandValidator : AbstractValidator<RemovePartyCommand>
{
    public RemovePartyCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.PartyId).NotEmpty();
    }
}

public sealed class RemovePartyCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<RemovePartyCommand, Result<PartyDto>>
{
    public async Task<Result<PartyDto>> Handle(RemovePartyCommand command, CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<PartyDto>();
        }

        var claim = loaded.Value;

        bool hasParty = claim.Parties.Any(party => party.Id == command.PartyId);

        if (!hasParty)
        {
            return Result<PartyDto>.Failure([ClaimErrors.PartyNotFound(command.PartyId)]);
        }

        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var outcome = claim.RemoveParty(command.ToInput(period, codes));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<PartyDto>(value));
    }
}
