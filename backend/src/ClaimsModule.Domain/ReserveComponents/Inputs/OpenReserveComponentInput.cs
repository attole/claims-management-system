using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Domain.ReserveComponents.Inputs;

public sealed record OpenReserveComponentInput(ClaimReference Claim, ReserveComponentType ComponentType, string Notes);
