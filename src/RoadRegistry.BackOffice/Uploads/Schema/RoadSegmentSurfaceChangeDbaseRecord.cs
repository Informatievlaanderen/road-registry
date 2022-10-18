namespace RoadRegistry.BackOffice.Uploads.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentSurfaceChangeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentSurfaceChangeDbaseSchema Schema = new();

    public RoadSegmentSurfaceChangeDbaseRecord()
    {
        WV_OIDN = new DbaseInt32(Schema.WV_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        VANPOSITIE = new DbaseDouble(Schema.VANPOSITIE);
        TOTPOSITIE = new DbaseDouble(Schema.TOTPOSITIE);
        TYPE = new DbaseInt16(Schema.TYPE);
        TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
        RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);

        Values = new DbaseFieldValue[]
        {
            WV_OIDN,
            WS_OIDN,
            VANPOSITIE,
            TOTPOSITIE,
            TYPE,
            TRANSACTID,
            RECORDTYPE
        };
    }

    public DbaseInt16 RECORDTYPE { get; }

    public DbaseDouble TOTPOSITIE { get; }

    public DbaseInt16 TRANSACTID { get; }

    public DbaseInt16 TYPE { get; }

    public DbaseDouble VANPOSITIE { get; }

    public DbaseInt32 WS_OIDN { get; }

    public DbaseInt32 WV_OIDN { get; }
}