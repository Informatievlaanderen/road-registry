namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentLaneAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentLaneAttributeDbaseSchema Schema = new();

    public RoadSegmentLaneAttributeDbaseRecord()
    {
        RS_OIDN = new DbaseInt32(Schema.RS_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        AANTAL = new DbaseInt32(Schema.AANTAL);
        RICHTING = new DbaseInt32(Schema.RICHTING);
        VANPOS = new DbaseDouble(Schema.VANPOS);
        TOTPOS = new DbaseDouble(Schema.TOTPOS);

        Values = new DbaseFieldValue[]
        {
            RS_OIDN,
            WS_OIDN,
            AANTAL,
            RICHTING,
            VANPOS,
            TOTPOS
        };
    }

    public DbaseInt32 AANTAL { get; set; }
    public DbaseInt32 RICHTING { get; set; }
    public DbaseInt32 RS_OIDN { get; set; }
    public DbaseDouble TOTPOS { get; set; }
    public DbaseDouble VANPOS { get; set; }
    public DbaseInt32 WS_OIDN { get; set; }
}
