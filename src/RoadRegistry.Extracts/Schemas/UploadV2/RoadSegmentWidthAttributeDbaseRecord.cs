namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentWidthAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentWidthAttributeDbaseSchema Schema = new();

    public RoadSegmentWidthAttributeDbaseRecord()
    {
        WB_OIDN = new DbaseInt32(Schema.WB_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        BREEDTE = new DbaseInt32(Schema.BREEDTE);
        VANPOS = new DbaseDouble(Schema.VANPOS);
        TOTPOS = new DbaseDouble(Schema.TOTPOS);

        Values = new DbaseFieldValue[]
        {
            WB_OIDN,
            WS_OIDN,
            BREEDTE,
            VANPOS,
            TOTPOS,
        };
    }

    public DbaseInt32 BREEDTE { get; set; }
    public DbaseDouble TOTPOS { get; set; }
    public DbaseDouble VANPOS { get; set; }
    public DbaseInt32 WB_OIDN { get; set; }
    public DbaseInt32 WS_OIDN { get; set; }
}
