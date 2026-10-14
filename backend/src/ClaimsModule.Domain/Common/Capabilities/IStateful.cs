namespace ClaimsModule.Domain.Common.Capabilities;

public interface IStateful<out TState>
{
    TState State { get; }
}
