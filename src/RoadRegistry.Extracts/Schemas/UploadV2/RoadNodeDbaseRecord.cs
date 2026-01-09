namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeDbaseRecord : DbaseRecord
{
    public static readonly RoadNodeDbaseSchema Schema = new();

    public RoadNodeDbaseRecord()
    {
        WK_OIDN = new DbaseInt32(Schema.WK_OIDN);
        TYPE = new DbaseInt32(Schema.TYPE);

        Values = new DbaseFieldValue[]
        {
            WK_OIDN, TYPE
        };
    }

    public DbaseInt32 TYPE { get; }
    public DbaseInt32 WK_OIDN { get; }
}
