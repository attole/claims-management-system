namespace ClaimsModule.Application.Abstractions;

public interface ICorrelationContext
{
    Guid CorrelationId { get; }
}
