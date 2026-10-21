namespace ClaimsModule.API.DevAuth;

public sealed record DevTokenDto(string Token, DateTimeOffset ExpiresAt, Guid UserId, string Name, string Role);
