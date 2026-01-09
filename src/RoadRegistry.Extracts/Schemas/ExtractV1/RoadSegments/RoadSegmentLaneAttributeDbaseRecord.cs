namespace RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentLaneAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentLaneAttributeDbaseSchema Schema = new();

    public RoadSegmentLaneAttributeDbaseRecord()
    {
        RS_OIDN = new DbaseInt32(Schema.RS_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        WS_GIDN = new TrimmedDbaseString(Schema.WS_GIDN);
        AANTAL = new DbaseInt32(Schema.AANTAL);
        RICHTING = new DbaseInt32(Schema.RICHTING);
        LBLRICHT = new TrimmedDbaseString(Schema.LBLRICHT);
        VANPOS = new DbaseNullableDouble(Schema.VANPOS);
        TOTPOS = new DbaseNullableDouble(Schema.TOTPOS);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new TrimmedDbaseString(Schema.BEGINORG);
        LBLBGNORG = new TrimmedDbaseString(Schema.LBLBGNORG);

        Values = new DbaseFieldValue[]
        {
            RS_OIDN,
            WS_OIDN,
            WS_GIDN,
            AANTAL,
            RICHTING,
            LBLRICHT,
            VANPOS,
            TOTPOS,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        };
    }

    public DbaseInt32 AANTAL { get; set; }
    public DbaseString BEGINORG { get; set; }
    public DbaseDateTime BEGINTIJD { get; set; }
    public DbaseString LBLBGNORG { get; set; }
    public DbaseString LBLRICHT { get; set; }
    public DbaseInt32 RICHTING { get; set; }
    public DbaseInt32 RS_OIDN { get; set; }
    public DbaseNullableDouble TOTPOS { get; set; }
    public DbaseNullableDouble VANPOS { get; set; }
    public DbaseString WS_GIDN { get; set; }
    public DbaseInt32 WS_OIDN { get; set; }
}
