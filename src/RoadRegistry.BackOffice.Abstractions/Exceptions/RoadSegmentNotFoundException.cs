namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class RoadSegmentNotFoundException : ApplicationException
{
    public RoadSegmentNotFoundException(string? message = null) : base(message ?? "Road segment could not be found.")
    {
    }

    private RoadSegmentNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
