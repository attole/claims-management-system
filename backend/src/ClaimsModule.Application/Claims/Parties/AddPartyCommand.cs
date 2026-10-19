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

public sealed record AddPartyCommand(Guid ClaimId, PartyRequest Party) : ICommand<Result<PartyDto>>
{
    public AddPartyInput ToInput(PolicyPeriod period, CauseCodes codes)
    {
        return new AddPartyInput(Party.ToDetails(), period, codes);
    }
}

public sealed class AddPartyCommandValidator : AbstractValidator<AddPartyCommand>
{
    public AddPartyCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.Party).NotNull().SetValidator(new PartyRequestValidator());
    }
}

public sealed class AddPartyCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<AddPartyCommand, Result<PartyDto>>
{
    public async Task<Result<PartyDto>> Handle(AddPartyCommand command, CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<PartyDto>();
        }

        var claim = loaded.Value;
        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var outcome = claim.AddParty(command.ToInput(period, codes));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<PartyDto>(value));
    }
}
