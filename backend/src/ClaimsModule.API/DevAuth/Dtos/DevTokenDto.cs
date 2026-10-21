namespace ClaimsModule.API.DevAuth.Dtos;

public sealed record DevTokenDto(string Token, DateTimeOffset ExpiresAt, Guid UserId, string Name, string Role);
