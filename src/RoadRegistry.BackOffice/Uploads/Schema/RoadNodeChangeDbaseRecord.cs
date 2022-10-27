namespace RoadRegistry.BackOffice.Uploads.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeChangeDbaseRecord : DbaseRecord
{
    public static readonly RoadNodeChangeDbaseSchema Schema = new();

    public RoadNodeChangeDbaseRecord()
    {
        WEGKNOOPID = new DbaseInt32(Schema.WEGKNOOPID);
        TYPE = new DbaseInt16(Schema.TYPE);
        TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
        RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);

        Values = new DbaseFieldValue[]
        {
            WEGKNOOPID,
            TYPE,
            TRANSACTID,
            RECORDTYPE
        };
    }

    public DbaseInt16 RECORDTYPE { get; }
    public DbaseInt16 TRANSACTID { get; }
    public DbaseInt16 TYPE { get; }
    public DbaseInt32 WEGKNOOPID { get; }
}