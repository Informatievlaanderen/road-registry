// ReSharper disable InconsistentNaming

namespace RoadRegistry.Product.Schema.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase;

public class RoadSegmentDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentDbaseSchema Schema = new();

    public RoadSegmentDbaseRecord()
    {
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        WS_UIDN = new DbaseString(Schema.WS_UIDN);
        WS_GIDN = new DbaseString(Schema.WS_GIDN);
        B_WK_OIDN = new DbaseInt32(Schema.B_WK_OIDN);
        E_WK_OIDN = new DbaseInt32(Schema.E_WK_OIDN);
        STATUS = new DbaseInt32(Schema.STATUS);
        LBLSTATUS = new DbaseString(Schema.LBLSTATUS);
        MORF = new DbaseInt32(Schema.MORF);
        LBLMORF = new DbaseString(Schema.LBLMORF);
        WEGCAT = new DbaseString(Schema.WEGCAT);
        LBLWEGCAT = new DbaseString(Schema.LBLWEGCAT);
        LSTRNMID = new DbaseNullableInt32(Schema.LSTRNMID);
        LSTRNM = new DbaseString(Schema.LSTRNM);
        RSTRNMID = new DbaseNullableInt32(Schema.RSTRNMID);
        RSTRNM = new DbaseString(Schema.RSTRNM);
        BEHEER = new DbaseString(Schema.BEHEER);
        LBLBEHEER = new DbaseString(Schema.LBLBEHEER);
        METHODE = new DbaseInt32(Schema.METHODE);
        LBLMETHOD = new DbaseString(Schema.LBLMETHOD);
        OPNDATUM = new DbaseDateTime(Schema.OPNDATUM);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);
        TGBEP = new DbaseInt32(Schema.TGBEP);
        LBLTGBEP = new DbaseString(Schema.LBLTGBEP);

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
}