namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;

public class RoadSegmentChangeDbaseRecord : DbaseRecord, IRoadSegmentDbaseRecord
{
    public static readonly RoadSegmentChangeDbaseSchema Schema = new();

    public RoadSegmentChangeDbaseRecord()
    {
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        METHODE = new DbaseInt32(Schema.METHODE);
        BEHEERDER = new TrimmedDbaseString(Schema.BEHEERDER);
        MORFOLOGIE = new DbaseInt32(Schema.MORFOLOGIE);
        STATUS = new DbaseInt32(Schema.STATUS);
        CATEGORIE = new TrimmedDbaseString(Schema.CATEGORIE);
        B_WK_OIDN = new DbaseInt32(Schema.B_WK_OIDN);
        E_WK_OIDN = new DbaseInt32(Schema.E_WK_OIDN);
        LSTRNMID = new DbaseNullableInt32(Schema.LSTRNMID);
        RSTRNMID = new DbaseNullableInt32(Schema.RSTRNMID);
        TGBEP = new DbaseInt32(Schema.TGBEP);
        TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
        RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);
        EVENTIDN = new DbaseInt32(Schema.EVENTIDN);

        Values = new DbaseFieldValue[]
        {
            WS_OIDN,
            METHODE,
            BEHEERDER,
            MORFOLOGIE,
            STATUS,
            CATEGORIE,
            B_WK_OIDN,
            E_WK_OIDN,
            LSTRNMID,
            RSTRNMID,
            TGBEP,
            TRANSACTID,
            RECORDTYPE,
            EVENTIDN
        };
    }

    public DbaseInt32 B_WK_OIDN { get; }
    public DbaseString BEHEERDER { get; }
    public DbaseString CATEGORIE { get; }
    public DbaseInt32 E_WK_OIDN { get; }
    public DbaseInt32 EVENTIDN { get; }
    public DbaseNullableInt32 LSTRNMID { get; }
    public DbaseInt32 METHODE { get; }
    public DbaseInt32 MORFOLOGIE { get; }
    public DbaseInt16 RECORDTYPE { get; }
    public DbaseNullableInt32 RSTRNMID { get; }
    public DbaseInt32 STATUS { get; }
    public DbaseInt32 TGBEP { get; }
    public DbaseInt16 TRANSACTID { get; }
    public DbaseInt32 WS_OIDN { get; }
}
