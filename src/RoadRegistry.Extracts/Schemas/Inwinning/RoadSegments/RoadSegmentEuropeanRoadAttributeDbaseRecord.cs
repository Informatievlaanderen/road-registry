namespace RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentEuropeanRoadAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentEuropeanRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentEuropeanRoadAttributeDbaseRecord()
    {
        EU_OIDN = new DbaseInt32(Schema.EU_OIDN);
        WS_TEMPID = new DbaseInt32(Schema.WS_TEMPID);
        EUNUMMER = new TrimmedDbaseString(Schema.EUNUMMER);
        CREATIE = new DbaseDateTime(Schema.CREATIE);

        Values =
        [
            EU_OIDN,
            WS_TEMPID,
            EUNUMMER,
            CREATIE
        ];
    }

    public DbaseInt32 EU_OIDN { get; }
    public DbaseInt32 WS_TEMPID { get; }
    public DbaseString EUNUMMER { get; }
    public DbaseDateTime CREATIE { get; }
}
