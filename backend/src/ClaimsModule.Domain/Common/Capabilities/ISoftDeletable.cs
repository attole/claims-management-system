namespace ClaimsModule.Domain.Common.Capabilities;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTimeOffset DeletedAt { get; set; }
}
