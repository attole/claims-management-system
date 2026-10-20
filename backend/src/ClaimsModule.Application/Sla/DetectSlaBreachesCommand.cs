using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Sla;

public sealed record DetectSlaBreachesCommand : ICommand<Result<SlaScanDto>>;

public sealed class DetectSlaBreachesCommandValidator : AbstractValidator<DetectSlaBreachesCommand>;

public sealed class DetectSlaBreachesCommandHandler(
    ISlaClaimReader reader,
    IAuditLogService audit,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICorrelationContext correlation) : IRequestHandler<DetectSlaBreachesCommand, Result<SlaScanDto>>
{
    private const string Description = "Claim has not been updated in 48 hours";

    private static readonly TimeSpan _staleAfter = TimeSpan.FromHours(48);
    private static readonly TimeSpan _dedupeWindow = TimeSpan.FromHours(24);

    public async Task<Result<SlaScanDto>> Handle(DetectSlaBreachesCommand command, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var staleBefore = now - _staleAfter;
        var flaggedSince = now - _dedupeWindow;
        var claimIds = await reader.ListBreachedClaimIdsAsync(staleBefore, flaggedSince, cancellationToken);
        var context = new AuditContext(now, Guid.Empty, correlation.CorrelationId);

        foreach (var claimId in claimIds)
        {
            var entry = new AuditEntry(claimId, AuditEventTypes.SlaBreachDetected, Description)
            {
                RelatedEntityId = claimId,
                RelatedEntityType = nameof(Claim),
            };

            await audit.WriteAsync(entry, context, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result<SlaScanDto>.Success(new SlaScanDto { BreachCount = claimIds.Count });
    }
}
