namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class GradeSeparatedJunctionDbaseRecord : DbaseRecord
{
    public static readonly GradeSeparatedJunctionDbaseSchema Schema = new();

    public GradeSeparatedJunctionDbaseRecord()
    {
        OK_OIDN = new DbaseInt32(Schema.OK_OIDN);
        TYPE = new DbaseInt32(Schema.TYPE);
        BO_WS_OIDN = new DbaseInt32(Schema.BO_WS_OIDN);
        ON_WS_OIDN = new DbaseInt32(Schema.ON_WS_OIDN);

        Values = new DbaseFieldValue[]
        {
            OK_OIDN,
            TYPE,
            BO_WS_OIDN,
            ON_WS_OIDN,
        };
    }

    public DbaseInt32 BO_WS_OIDN { get; }
    public DbaseInt32 OK_OIDN { get; }
    public DbaseInt32 ON_WS_OIDN { get; }
    public DbaseInt32 TYPE { get; }
}
