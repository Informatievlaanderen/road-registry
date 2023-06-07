namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;
using BackOffice.Exceptions;

[Serializable]
public class RoadSegmentNotFoundException : RoadRegistryException
{
    public RoadSegmentNotFoundException() : base("Road segment could not be found.")
    {
    }

    public RoadSegmentNotFoundException(string message) : base(message)
    {
    }

    protected RoadSegmentNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
