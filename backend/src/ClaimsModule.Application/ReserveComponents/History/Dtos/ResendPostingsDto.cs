namespace ClaimsModule.Application.ReserveComponents.History.Dtos;

public sealed record ResendPostingsDto
{
    public int ResentCount { get; init; } = 0;
}
