namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class EuropeanRoadChangeDbaseRecord : DbaseRecord
{
    public static readonly EuropeanRoadChangeDbaseSchema Schema = new();

    public EuropeanRoadChangeDbaseRecord()
    {
        EU_OIDN = new DbaseInt32(Schema.EU_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        EUNUMMER = new TrimmedDbaseString(Schema.EUNUMMER);
        TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
        RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);

        Values = new DbaseFieldValue[]
        {
            EU_OIDN,
            WS_OIDN,
            EUNUMMER,
            TRANSACTID,
            RECORDTYPE
        };
    }

    public DbaseInt32 EU_OIDN { get; }
    public DbaseString EUNUMMER { get; }
    public DbaseInt16 RECORDTYPE { get; }
    public DbaseInt16 TRANSACTID { get; }
    public DbaseInt32 WS_OIDN { get; }
}
