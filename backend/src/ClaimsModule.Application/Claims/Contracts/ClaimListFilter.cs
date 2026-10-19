using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Application.Claims.Contracts;

public sealed record ClaimListFilter
{
    public IReadOnlyList<ClaimStateKind> Statuses { get; init; } = [];

    public DateOnly DateFrom { get; init; } = DateOnly.MinValue;

    public DateOnly DateTo { get; init; } = DateOnly.MaxValue;

    public Guid AssignedHandlerId { get; init; } = Guid.Empty;

    public string CauseOfLossCode { get; init; } = string.Empty;

    public Guid PolicyId { get; init; } = Guid.Empty;

    public string Search { get; init; } = string.Empty;
}
