using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Domain.ClaimDocuments.Inputs;

public sealed record RemoveClaimDocumentInput(ClaimState ClaimState);
