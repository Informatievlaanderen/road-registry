namespace RoadRegistry.Product.Schema.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentWidthAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentWidthAttributeDbaseSchema Schema = new();

    public RoadSegmentWidthAttributeDbaseRecord()
    {
        WB_OIDN = new DbaseInt32(Schema.WB_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        WS_GIDN = new DbaseString(Schema.WS_GIDN);
        BREEDTE = new DbaseInt32(Schema.BREEDTE);
        VANPOS = new DbaseDouble(Schema.VANPOS);
        TOTPOS = new DbaseDouble(Schema.TOTPOS);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

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

    public DbaseInt32 WB_OIDN { get; set; }
    public DbaseInt32 WS_OIDN { get; set; }
    public DbaseString WS_GIDN { get; set; }
    public DbaseInt32 BREEDTE { get; set; }
    public DbaseDouble VANPOS { get; set; }
    public DbaseDouble TOTPOS { get; set; }
    public DbaseDateTime BEGINTIJD { get; set; }
    public DbaseString BEGINORG { get; set; }
    public DbaseString LBLBGNORG { get; set; }
}
