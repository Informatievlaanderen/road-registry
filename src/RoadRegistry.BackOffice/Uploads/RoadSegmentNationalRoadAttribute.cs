namespace RoadRegistry.BackOffice.Uploads;

public class RoadSegmentNationalRoadAttribute
{
    public RoadSegmentNationalRoadAttribute(
        AttributeId temporaryId,
        NationalRoadNumber number
    )
    {
        TemporaryId = temporaryId;
        Number = number;
    }

    public AttributeId TemporaryId { get; }
    public NationalRoadNumber Number { get; }
}
