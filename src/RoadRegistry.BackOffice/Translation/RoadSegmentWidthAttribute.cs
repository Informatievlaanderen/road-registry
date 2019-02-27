namespace RoadRegistry.BackOffice.Translation
{
    using Model;

    public class RoadSegmentWidthAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentWidthAttribute(
            AttributeId temporaryId,
            RoadSegmentWidth width,
            RoadSegmentPosition from,
            RoadSegmentPosition to
        ) : base(temporaryId, from, to)
        {
            Width = width;
        }

        public RoadSegmentWidth Width { get; }
    }
}
