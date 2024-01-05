// ReSharper disable InconsistentNaming

namespace RoadRegistry.BackOffice.Uploads.Dbase.BeforeFeatureCompare.V2.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;

public class RoadSegmentDbaseRecord : DbaseRecord, IRoadSegmentDbaseRecord
{
    public static readonly RoadSegmentDbaseSchema Schema = new();

    public RoadSegmentDbaseRecord()
    {
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        B_WK_OIDN = new DbaseInt32(Schema.B_WK_OIDN);
        E_WK_OIDN = new DbaseInt32(Schema.E_WK_OIDN);
        STATUS = new DbaseInt32(Schema.STATUS);
        MORF = new DbaseInt32(Schema.MORF);
        WEGCAT = new TrimmedDbaseString(Schema.WEGCAT);
        LSTRNMID = new DbaseNullableInt32(Schema.LSTRNMID);
        RSTRNMID = new DbaseNullableInt32(Schema.RSTRNMID);
        BEHEER = new TrimmedDbaseString(Schema.BEHEER);
        METHODE = new DbaseInt32(Schema.METHODE);
        TGBEP = new DbaseInt32(Schema.TGBEP);

        Values = new DbaseFieldValue[]
        {
            WS_OIDN,
            B_WK_OIDN,
            E_WK_OIDN,
            STATUS,
            MORF,
            WEGCAT,
            LSTRNMID,
            RSTRNMID,
            BEHEER,
            METHODE,
            TGBEP
        };
    }

    public DbaseInt32 B_WK_OIDN { get; }
    public DbaseString BEHEER { get; }
    public DbaseInt32 E_WK_OIDN { get; }
    public DbaseNullableInt32 LSTRNMID { get; }
    public DbaseInt32 METHODE { get; }
    public DbaseInt32 MORF { get; }
    public DbaseNullableInt32 RSTRNMID { get; }
    public DbaseInt32 STATUS { get; }
    public DbaseInt32 TGBEP { get; }
    public DbaseString WEGCAT { get; }
    public DbaseInt32 WS_OIDN { get; }

    public DbaseString BEHEERDER => BEHEER;
    public DbaseInt32 MORFOLOGIE => MORF;
    public DbaseString CATEGORIE => WEGCAT;
}
