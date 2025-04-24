namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using BackOffice.Exceptions;

public class RoadSegmentNotFoundException : RoadRegistryException
{
    public RoadSegmentNotFoundException()
        : base("Road segment could not be found.")
    {
    }

    public RoadSegmentNotFoundException(string message)
        : base(message)
    {
    }
}
