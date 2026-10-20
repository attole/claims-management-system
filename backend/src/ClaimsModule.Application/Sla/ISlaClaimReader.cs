namespace ClaimsModule.Application.Sla;

public interface ISlaClaimReader
{
    Task<IReadOnlyList<Guid>> ListBreachedClaimIdsAsync(
        DateTimeOffset notUpdatedSince,
        DateTimeOffset notBreachedSince,
        CancellationToken cancellationToken);
}
