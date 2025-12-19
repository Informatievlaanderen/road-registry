// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentDbaseRecord : DbaseRecord, IRoadSegmentDbaseRecord
{
    public static readonly RoadSegmentDbaseSchema Schema = new();

    public RoadSegmentDbaseRecord()
    {
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        WS_UIDN = new TrimmedDbaseString(Schema.WS_UIDN);
        WS_GIDN = new TrimmedDbaseString(Schema.WS_GIDN);
        B_WK_OIDN = new DbaseInt32(Schema.B_WK_OIDN);
        E_WK_OIDN = new DbaseInt32(Schema.E_WK_OIDN);
        STATUS = new DbaseInt32(Schema.STATUS);
        LBLSTATUS = new TrimmedDbaseString(Schema.LBLSTATUS);
        MORF = new DbaseInt32(Schema.MORF);
        LBLMORF = new TrimmedDbaseString(Schema.LBLMORF);
        WEGCAT = new TrimmedDbaseString(Schema.WEGCAT);
        LBLWEGCAT = new TrimmedDbaseString(Schema.LBLWEGCAT);
        LSTRNMID = new DbaseNullableInt32(Schema.LSTRNMID);
        LSTRNM = new TrimmedDbaseString(Schema.LSTRNM);
        RSTRNMID = new DbaseNullableInt32(Schema.RSTRNMID);
        RSTRNM = new TrimmedDbaseString(Schema.RSTRNM);
        BEHEER = new TrimmedDbaseString(Schema.BEHEER);
        LBLBEHEER = new TrimmedDbaseString(Schema.LBLBEHEER);
        METHODE = new DbaseInt32(Schema.METHODE);
        LBLMETHOD = new TrimmedDbaseString(Schema.LBLMETHOD);
        OPNDATUM = new DbaseDateTime(Schema.OPNDATUM);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new TrimmedDbaseString(Schema.BEGINORG);
        LBLBGNORG = new TrimmedDbaseString(Schema.LBLBGNORG);
        TGBEP = new DbaseInt32(Schema.TGBEP);
        LBLTGBEP = new TrimmedDbaseString(Schema.LBLTGBEP);

        Values = new DbaseFieldValue[]
        {
            WS_OIDN,
            WS_UIDN,
            WS_GIDN,
            B_WK_OIDN,
            E_WK_OIDN,
            STATUS,
            LBLSTATUS,
            MORF,
            LBLMORF,
            WEGCAT,
            LBLWEGCAT,
            LSTRNMID,
            LSTRNM,
            RSTRNMID,
            RSTRNM,
            BEHEER,
            LBLBEHEER,
            METHODE,
            LBLMETHOD,
            OPNDATUM,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG,
            TGBEP,
            LBLTGBEP
        };
    }

    public DbaseInt32 B_WK_OIDN { get; }
    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString BEHEER { get; }
    public DbaseInt32 E_WK_OIDN { get; }
    public DbaseString LBLBEHEER { get; }
    public DbaseString LBLBGNORG { get; }
    public DbaseString LBLMETHOD { get; }
    public DbaseString LBLMORF { get; }
    public DbaseString LBLSTATUS { get; }
    public DbaseString LBLTGBEP { get; }
    public DbaseString LBLWEGCAT { get; }
    public DbaseString LSTRNM { get; }
    public DbaseNullableInt32 LSTRNMID { get; }
    public DbaseInt32 METHODE { get; }
    public DbaseInt32 MORF { get; }
    public DbaseDateTime OPNDATUM { get; }
    public DbaseString RSTRNM { get; }
    public DbaseNullableInt32 RSTRNMID { get; }
    public DbaseInt32 STATUS { get; }
    public DbaseInt32 TGBEP { get; }
    public DbaseString WEGCAT { get; }
    public DbaseString WS_GIDN { get; }
    public DbaseInt32 WS_OIDN { get; }
    public DbaseString WS_UIDN { get; }

    public DbaseString BEHEERDER => BEHEER;
    public DbaseInt32 MORFOLOGIE => MORF;
    public DbaseString CATEGORIE => WEGCAT;
}
