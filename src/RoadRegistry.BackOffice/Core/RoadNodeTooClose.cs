namespace RoadRegistry.BackOffice.Core
{
    public class RoadNodeTooClose : Error
    {
        public RoadNodeTooClose(RoadSegmentId toOtherSegment) :
            base(nameof(RoadNodeTooClose),
                new ProblemParameter(
                    "ToOtherSegment",
                    toOtherSegment.ToInt32().ToString()))
        {
        }
    }
}