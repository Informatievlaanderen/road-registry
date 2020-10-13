namespace RoadRegistry.BackOffice.Core
{
    public class RoadNodeNotConnectedToAnySegment : Error
    {
        public RoadNodeNotConnectedToAnySegment()
            : base(nameof(RoadNodeNotConnectedToAnySegment))
        {
        }
    }
}