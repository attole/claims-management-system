namespace ClaimsModule.Domain.Common.Capabilities;

public interface ITenantOwned
{
    Guid OrganisationId { get; }
}
