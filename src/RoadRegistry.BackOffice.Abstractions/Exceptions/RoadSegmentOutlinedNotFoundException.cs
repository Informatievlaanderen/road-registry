namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class RoadSegmentOutlinedNotFoundException : RoadSegmentNotFoundException
{
    public RoadSegmentOutlinedNotFoundException()
        : base("Road segment outlined could not be found.")
    {
    }

    public RoadSegmentOutlinedNotFoundException(string message) : base(message)
    {
    }
}
