using ClaimsModule.Application.Idempotency;
using ClaimsModule.Domain.Common.Capabilities;

namespace ClaimsModule.Persistence.Idempotency;

internal sealed class IdempotencyRecord : ITenantOwned
{
    private IdempotencyRecord()
    {
    }

    public IdempotencyRecord(
        Guid id,
        Guid organisationId,
        Guid userId,
        IdempotencyRequest request,
        DateTimeOffset createdAt)
    {
        Id = id;
        OrganisationId = organisationId;
        UserId = userId;
        Key = request.Key;
        Method = request.Method;
        Route = request.Route;
        BodyHash = request.BodyHash;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; } = Guid.Empty;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    public Guid UserId { get; private set; } = Guid.Empty;

    public string Key { get; private set; } = string.Empty;

    public string Method { get; private set; } = string.Empty;

    public string Route { get; private set; } = string.Empty;

    public string BodyHash { get; private set; } = string.Empty;

    public int ResponseStatusCode { get; private set; } = 0;

    public string ResponseBody { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public bool IsInProgress => ResponseStatusCode == 0;

    public IdempotencyResponse Response => new(ResponseStatusCode, ResponseBody);

    public void Restart(string bodyHash, DateTimeOffset now)
    {
        BodyHash = bodyHash;
        ResponseStatusCode = 0;
        ResponseBody = string.Empty;
        CreatedAt = now;
    }

    public void Complete(IdempotencyResponse response)
    {
        ResponseStatusCode = response.StatusCode;
        ResponseBody = response.Body;
    }
}
