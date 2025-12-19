namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentSurfaceAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentSurfaceAttributeDbaseSchema Schema = new();

    public RoadSegmentSurfaceAttributeDbaseRecord()
    {
        WV_OIDN = new DbaseInt32(Schema.WV_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        TYPE = new DbaseInt32(Schema.TYPE);
        VANPOS = new DbaseDouble(Schema.VANPOS);
        TOTPOS = new DbaseDouble(Schema.TOTPOS);

        Values = new DbaseFieldValue[]
        {
            WV_OIDN,
            WS_OIDN,
            TYPE,
            VANPOS,
            TOTPOS,
        };
    }

    public DbaseDouble TOTPOS { get; set; }
    public DbaseInt32 TYPE { get; set; }
    public DbaseDouble VANPOS { get; set; }
    public DbaseInt32 WS_OIDN { get; set; }
    public DbaseInt32 WV_OIDN { get; set; }
}
