namespace ClaimsModule.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
