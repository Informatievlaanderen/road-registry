namespace RoadRegistry.Extracts.Schemas.DomainV2.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentNationalRoadAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentNationalRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentNationalRoadAttributeDbaseRecord()
    {
        NW_OIDN = new DbaseInt32(Schema.NW_OIDN);
        WS_TEMPID = new DbaseInt32(Schema.WS_TEMPID);
        NWNUMMER = new TrimmedDbaseString(Schema.NWNUMMER);
        CREATIE = new DbaseDateTime(Schema.CREATIE);

        Values =
        [
            NW_OIDN,
            WS_TEMPID,
            NWNUMMER,
            CREATIE
        ];
    }

    public DbaseInt32 NW_OIDN { get; }
    public DbaseInt32 WS_TEMPID { get; }
    public DbaseString NWNUMMER { get; }
    public DbaseDateTime CREATIE { get; }
}
