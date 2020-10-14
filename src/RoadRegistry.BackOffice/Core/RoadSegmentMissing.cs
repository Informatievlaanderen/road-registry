namespace RoadRegistry.BackOffice.Core
{
    public class RoadSegmentMissing : Error
    {
        public RoadSegmentMissing(RoadSegmentId segmentId)
            : base(nameof(RoadSegmentMissing),
                new ProblemParameter("SegmentId", segmentId.ToInt32().ToString()))
        {
        }
    }
}