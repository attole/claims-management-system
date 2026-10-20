using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Idempotency;

public sealed record PurgeExpiredIdempotencyKeysCommand : ICommand<Result<IdempotencyPurgeDto>>;

public sealed class PurgeExpiredIdempotencyKeysCommandValidator : AbstractValidator<PurgeExpiredIdempotencyKeysCommand>;

public sealed class PurgeExpiredIdempotencyKeysCommandHandler(IIdempotencyStore store)
    : IRequestHandler<PurgeExpiredIdempotencyKeysCommand, Result<IdempotencyPurgeDto>>
{
    public async Task<Result<IdempotencyPurgeDto>> Handle(
        PurgeExpiredIdempotencyKeysCommand command,
        CancellationToken cancellationToken)
    {
        int purged = await store.PurgeExpiredAsync(cancellationToken);

        return Result<IdempotencyPurgeDto>.Success(new IdempotencyPurgeDto { PurgedCount = purged });
    }
}
