using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.ClaimDocuments.Inputs;

public sealed record UploadClaimDocumentInput(
    ClaimReference Claim,
    ClaimDocumentDetails Details,
    Guid ActorId,
    DateTimeOffset Now)
{
    public const int HeaderLength = 512;

    public Guid ActorId { get; } = InputActor.Required(ActorId);

    public DateTimeOffset Now { get; } = InputTime.Required(Now);

    public IReadOnlyList<byte> Header { get; init; } = [];
}
