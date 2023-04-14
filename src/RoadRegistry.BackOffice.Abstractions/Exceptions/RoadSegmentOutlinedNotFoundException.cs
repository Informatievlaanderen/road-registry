namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public class RoadSegmentOutlinedNotFoundException : RoadSegmentNotFoundException
{
    public RoadSegmentOutlinedNotFoundException() : base("Road segment outlined could not be found.")
    {
    }

    public RoadSegmentOutlinedNotFoundException(string message) : base(message)
    {
    }

    protected RoadSegmentOutlinedNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
