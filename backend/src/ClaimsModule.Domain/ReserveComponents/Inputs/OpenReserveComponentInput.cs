using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Domain.ReserveComponents.Inputs;

public sealed record OpenReserveComponentInput(
    ClaimReference Claim,
    ReserveComponentTypeKind ComponentTypeKind,
    string Notes);
