namespace RoadRegistry.BackOffice.Uploads.Schema.V1;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class GradeSeparatedJunctionChangeDbaseRecord : DbaseRecord
{
    public static readonly GradeSeparatedJunctionChangeDbaseSchema Schema = new();

    public GradeSeparatedJunctionChangeDbaseRecord()
    {
        OK_OIDN = new DbaseInt32(Schema.OK_OIDN);
        TYPE = new DbaseInt16(Schema.TYPE);
        BO_WS_OIDN = new DbaseInt32(Schema.BO_WS_OIDN);
        ON_WS_OIDN = new DbaseInt32(Schema.ON_WS_OIDN);
        TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
        RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);

        Values = new DbaseFieldValue[]
        {
            OK_OIDN,
            TYPE,
            BO_WS_OIDN,
            ON_WS_OIDN,
            TRANSACTID,
            RECORDTYPE
        };
    }

    public DbaseInt32 BO_WS_OIDN { get; }
    public DbaseInt32 OK_OIDN { get; }
    public DbaseInt32 ON_WS_OIDN { get; }
    public DbaseInt16 RECORDTYPE { get; }
    public DbaseInt16 TRANSACTID { get; }
    public DbaseInt16 TYPE { get; }
}
