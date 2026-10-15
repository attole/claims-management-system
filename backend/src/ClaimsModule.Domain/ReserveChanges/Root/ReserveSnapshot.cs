using ClaimsModule.Domain.Reserves.Root;

namespace ClaimsModule.Domain.ReserveChanges.Root;

public sealed record ReserveSnapshot(
    bool IsPolicyLinked,
    decimal ClaimApprovedTotal,
    bool HasLimitOverride,
    ReserveComponentType ComponentType,
    decimal ReserveApprovedBalance,
    int NextChangeSequence);
