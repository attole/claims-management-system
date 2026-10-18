using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.ClaimDocuments.Inputs;

public sealed record RemoveClaimDocumentInput(ClaimState ClaimState, DateTimeOffset Now)
{
    public DateTimeOffset Now { get; } = InputTime.Required(Now);
}
