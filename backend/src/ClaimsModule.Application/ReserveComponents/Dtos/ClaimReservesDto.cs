using ClaimsModule.Application.ReserveComponents.History.Dtos;

namespace ClaimsModule.Application.ReserveComponents.Dtos;

public sealed record ClaimReservesDto
{
    public decimal ApprovedTotal { get; init; } = 0m;

    public IReadOnlyList<ReserveComponentDto> Components { get; init; } = [];

    public IReadOnlyList<ReserveHistoryDto> History { get; init; } = [];
}
