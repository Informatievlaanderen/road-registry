namespace RoadRegistry.BackOffice.Uploads;

public class RoadSegmentNumberedRoadAttribute
{
    public RoadSegmentNumberedRoadAttribute(
        AttributeId temporaryId,
        NumberedRoadNumber number,
        RoadSegmentNumberedRoadDirection direction,
        RoadSegmentNumberedRoadOrdinal ordinal
    )
    {
        TemporaryId = temporaryId;
        Number = number;
        Direction = direction;
        Ordinal = ordinal;
    }

    public AttributeId TemporaryId { get; }
    public NumberedRoadNumber Number { get; }
    public RoadSegmentNumberedRoadDirection Direction { get; set; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; set; }
}
