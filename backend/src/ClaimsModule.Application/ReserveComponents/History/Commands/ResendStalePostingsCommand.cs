using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ReserveComponents.History.Commands;

public sealed record ResendStalePostingsCommand(TimeSpan MinimumAge) : ICommand<Result<ResendPostingsDto>>;

public sealed class ResendStalePostingsCommandValidator : AbstractValidator<ResendStalePostingsCommand>
{
    public ResendStalePostingsCommandValidator()
    {
        RuleFor(command => command.MinimumAge).GreaterThan(TimeSpan.Zero);
    }
}

public sealed class ResendStalePostingsCommandHandler(
    IClaimReserveReader reader,
    IPostingScheduler scheduler,
    IClock clock) : IRequestHandler<ResendStalePostingsCommand, Result<ResendPostingsDto>>
{
    public async Task<Result<ResendPostingsDto>> Handle(
        ResendStalePostingsCommand command,
        CancellationToken cancellationToken)
    {
        var approvedBefore = clock.UtcNow - command.MinimumAge;
        var unposted = await reader.ListStalePendingPostingsAsync(approvedBefore, cancellationToken);

        foreach (var reserve in unposted)
        {
            await scheduler.EnqueueAsync(reserve.ClaimId, reserve.ReserveHistoryId, string.Empty, cancellationToken);
        }

        return Result<ResendPostingsDto>.Success(new ResendPostingsDto { ResentCount = unposted.Count });
    }
}
