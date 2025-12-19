namespace RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentNationalRoadAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentNationalRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentNationalRoadAttributeDbaseRecord()
    {
        NW_OIDN = new DbaseInt32(Schema.NW_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        IDENT2 = new TrimmedDbaseString(Schema.IDENT2);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new TrimmedDbaseString(Schema.BEGINORG);
        LBLBGNORG = new TrimmedDbaseString(Schema.LBLBGNORG);

        Values = new DbaseFieldValue[]
        {
            NW_OIDN,
            WS_OIDN,
            IDENT2,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        };
    }

    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString IDENT2 { get; }
    public DbaseString LBLBGNORG { get; }
    public DbaseInt32 NW_OIDN { get; }
    public DbaseInt32 WS_OIDN { get; }
}
