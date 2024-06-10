namespace RoadRegistry.BackOffice.Uploads;

public class RoadSegmentEuropeanRoadAttribute
{
    public RoadSegmentEuropeanRoadAttribute(
        AttributeId temporaryId,
        EuropeanRoadNumber number
    )
    {
        TemporaryId = temporaryId;
        Number = number;
    }

    public AttributeId TemporaryId { get; }
    public EuropeanRoadNumber Number { get; }
}
