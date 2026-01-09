namespace RoadRegistry.Extracts.Schemas.ExtractV2.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentSurfaceAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentSurfaceAttributeDbaseSchema Schema = new();

    public RoadSegmentSurfaceAttributeDbaseRecord()
    {
        WV_OIDN = new DbaseInt32(Schema.WV_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        WS_GIDN = new TrimmedDbaseString(Schema.WS_GIDN);
        TYPE = new DbaseInt32(Schema.TYPE);
        LBLTYPE = new TrimmedDbaseString(Schema.LBLTYPE);
        VANPOS = new DbaseNullableDouble(Schema.VANPOS);
        TOTPOS = new DbaseNullableDouble(Schema.TOTPOS);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new TrimmedDbaseString(Schema.BEGINORG);
        LBLBGNORG = new TrimmedDbaseString(Schema.LBLBGNORG);

        Values = new DbaseFieldValue[]
        {
            WV_OIDN,
            WS_OIDN,
            WS_GIDN,
            TYPE,
            LBLTYPE,
            VANPOS,
            TOTPOS,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        };
    }

    public DbaseString BEGINORG { get; set; }
    public DbaseDateTime BEGINTIJD { get; set; }
    public DbaseString LBLBGNORG { get; set; }
    public DbaseString LBLTYPE { get; set; }
    public DbaseNullableDouble TOTPOS { get; set; }
    public DbaseInt32 TYPE { get; set; }
    public DbaseNullableDouble VANPOS { get; set; }
    public DbaseString WS_GIDN { get; set; }
    public DbaseInt32 WS_OIDN { get; set; }
    public DbaseInt32 WV_OIDN { get; set; }
}
