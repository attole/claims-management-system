using ClaimsModule.Application.Abstractions;

namespace ClaimsModule.API.Correlation;

internal sealed class CorrelationContext : ICorrelationContext
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
}
