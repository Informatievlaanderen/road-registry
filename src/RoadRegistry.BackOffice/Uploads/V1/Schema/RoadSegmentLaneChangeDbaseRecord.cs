namespace RoadRegistry.BackOffice.Uploads.Schema.V1;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentLaneChangeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentLaneChangeDbaseSchema Schema = new();

    public RoadSegmentLaneChangeDbaseRecord()
    {
        RS_OIDN = new DbaseInt32(Schema.RS_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        VANPOSITIE = new DbaseDouble(Schema.VANPOSITIE);
        TOTPOSITIE = new DbaseDouble(Schema.TOTPOSITIE);
        AANTAL = new DbaseInt16(Schema.AANTAL);
        RICHTING = new DbaseInt16(Schema.RICHTING);
        TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
        RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);

        Values = new DbaseFieldValue[]
        {
            RS_OIDN,
            WS_OIDN,
            VANPOSITIE,
            TOTPOSITIE,
            AANTAL,
            RICHTING,
            TRANSACTID,
            RECORDTYPE
        };
    }

    public DbaseInt16 AANTAL { get; }
    public DbaseInt16 RECORDTYPE { get; }
    public DbaseInt16 RICHTING { get; }
    public DbaseInt32 RS_OIDN { get; }
    public DbaseDouble TOTPOSITIE { get; }
    public DbaseInt16 TRANSACTID { get; }
    public DbaseDouble VANPOSITIE { get; }
    public DbaseInt32 WS_OIDN { get; }
}
