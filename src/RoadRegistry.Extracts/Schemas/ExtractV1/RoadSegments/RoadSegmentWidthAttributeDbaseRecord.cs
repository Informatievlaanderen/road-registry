namespace RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentWidthAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentWidthAttributeDbaseSchema Schema = new();

    public RoadSegmentWidthAttributeDbaseRecord()
    {
        WB_OIDN = new DbaseInt32(Schema.WB_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        WS_GIDN = new TrimmedDbaseString(Schema.WS_GIDN);
        BREEDTE = new DbaseInt32(Schema.BREEDTE);
        VANPOS = new DbaseNullableDouble(Schema.VANPOS);
        TOTPOS = new DbaseNullableDouble(Schema.TOTPOS);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new TrimmedDbaseString(Schema.BEGINORG);
        LBLBGNORG = new TrimmedDbaseString(Schema.LBLBGNORG);

        Values = new DbaseFieldValue[]
        {
            WB_OIDN,
            WS_OIDN,
            WS_GIDN,
            BREEDTE,
            VANPOS,
            TOTPOS,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        };
    }

    public DbaseString BEGINORG { get; set; }
    public DbaseDateTime BEGINTIJD { get; set; }
    public DbaseInt32 BREEDTE { get; set; }
    public DbaseString LBLBGNORG { get; set; }
    public DbaseNullableDouble TOTPOS { get; set; }
    public DbaseNullableDouble VANPOS { get; set; }
    public DbaseInt32 WB_OIDN { get; set; }
    public DbaseString WS_GIDN { get; set; }
    public DbaseInt32 WS_OIDN { get; set; }
}
