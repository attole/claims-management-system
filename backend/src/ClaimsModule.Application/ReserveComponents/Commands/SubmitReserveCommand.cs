using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.ReserveComponents.Requests;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ReserveComponents.Commands;

public sealed record SubmitReserveCommand(Guid ClaimId, ReserveRequest Reserve)
    : ICommand<Result<ReserveHistoryDto>>
{
    public SubmitHistoryInput ToInput(Claim claim, decimal approvedTotal, Guid actorId, DateTimeOffset now)
    {
        return Reserve.ToSubmitInput(claim, approvedTotal, actorId, now);
    }
}

public sealed class SubmitReserveCommandValidator : AbstractValidator<SubmitReserveCommand>
{
    public SubmitReserveCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.Reserve).NotNull().SetValidator(new ReserveRequestValidator());
    }
}

public sealed class SubmitReserveCommandHandler(
    IClaimRepository claims,
    IReserveComponentRepository components,
    IClaimReserveReader reserves,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user,
    IMapper mapper) : IRequestHandler<SubmitReserveCommand, Result<ReserveHistoryDto>>
{
    public async Task<Result<ReserveHistoryDto>> Handle(
        SubmitReserveCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ReserveHistoryDto>();
        }

        var claim = loaded.Value;
        var component = await LoadComponentAsync(command, claim, cancellationToken);

        if (!component.IsSuccess)
        {
            return component.ToFailure<ReserveHistoryDto>();
        }

        var facts = await reserves.GetFactsAsync(claim.Id, cancellationToken);
        var input = command.ToInput(claim, facts.ApprovedTotal, user.Id, clock.UtcNow);
        var result = component.Value.SubmitHistory(input).ToResult();

        if (result.IsSuccess)
        {
            await StoreAsync(command, component.Value, cancellationToken);
        }

        return result.Map(history => mapper.Map<ReserveHistoryDto>(history));
    }

    private async Task<Result<ReserveComponent>> LoadComponentAsync(
        SubmitReserveCommand command,
        Claim claim,
        CancellationToken cancellationToken)
    {
        var reserve = command.Reserve;

        return reserve.IsNewComponent
            ? ReserveComponent.Open(reserve.ToOpenInput(claim.Reference)).ToResult()
            : await components.GetAsync(claim.Id, reserve.ComponentId, cancellationToken);
    }

    private async Task StoreAsync(
        SubmitReserveCommand command,
        ReserveComponent component,
        CancellationToken cancellationToken)
    {
        if (command.Reserve.IsNewComponent)
        {
            await components.AddAsync(component, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
