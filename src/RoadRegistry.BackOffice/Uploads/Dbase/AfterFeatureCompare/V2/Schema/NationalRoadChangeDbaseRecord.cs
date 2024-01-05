namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class NationalRoadChangeDbaseRecord : DbaseRecord
{
    public static readonly NationalRoadChangeDbaseSchema Schema = new();

    public NationalRoadChangeDbaseRecord()
    {
        NW_OIDN = new DbaseInt32(Schema.NW_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        IDENT2 = new TrimmedDbaseString(Schema.IDENT2);
        TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
        RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);

        Values = new DbaseFieldValue[]
        {
            NW_OIDN,
            WS_OIDN,
            IDENT2,
            TRANSACTID,
            RECORDTYPE
        };
    }

    public DbaseString IDENT2 { get; }
    public DbaseInt32 NW_OIDN { get; }
    public DbaseInt16 RECORDTYPE { get; }
    public DbaseInt16 TRANSACTID { get; }
    public DbaseInt32 WS_OIDN { get; }
}
