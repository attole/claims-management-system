namespace ClaimsModule.Domain.Common.Markers;

public abstract record Marked<TMarker>(TMarker Marker) : IMarked where TMarker : struct, Enum
{
    public string Name => Marker.ToString();
}
