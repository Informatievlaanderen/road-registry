namespace RoadRegistry.Editor.Schema.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentSurfaceAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentSurfaceAttributeDbaseSchema Schema = new();

    public RoadSegmentSurfaceAttributeDbaseRecord()
    {
        WV_OIDN = new DbaseInt32(Schema.WV_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        WS_GIDN = new DbaseString(Schema.WS_GIDN);
        TYPE = new DbaseInt32(Schema.TYPE);
        LBLTYPE = new DbaseString(Schema.LBLTYPE);
        VANPOS = new DbaseDouble(Schema.VANPOS);
        TOTPOS = new DbaseDouble(Schema.TOTPOS);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

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
    public DbaseDouble TOTPOS { get; set; }
    public DbaseInt32 TYPE { get; set; }
    public DbaseDouble VANPOS { get; set; }
    public DbaseString WS_GIDN { get; set; }
    public DbaseInt32 WS_OIDN { get; set; }
    public DbaseInt32 WV_OIDN { get; set; }
}