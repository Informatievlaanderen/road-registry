// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentDbaseSchema Schema = new();

    public RoadSegmentDbaseRecord()
    {
        WS_TEMPID = new DbaseInt32(Schema.WS_TEMPID);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        STATUS = new DbaseInt32(Schema.STATUS);
        MORF = new DbaseInt32(Schema.MORF);
        WEGCAT = new TrimmedDbaseString(Schema.WEGCAT);
        LSTRNMID = new DbaseNullableInt32(Schema.LSTRNMID);
        RSTRNMID = new DbaseNullableInt32(Schema.RSTRNMID);
        LBEHEER = new TrimmedDbaseString(Schema.LBEHEER);
        RBEHEER = new TrimmedDbaseString(Schema.RBEHEER);
        TOEGANG = new DbaseInt32(Schema.TOEGANG);
        VERHARDING = new DbaseInt32(Schema.VERHARDING);
        AUTOHEEN = new DbaseNullableInt32(Schema.AUTOHEEN);
        AUTOTERUG = new DbaseNullableInt32(Schema.AUTOTERUG);
        FIETSHEEN = new DbaseNullableInt32(Schema.FIETSHEEN);
        FIETSTERUG = new DbaseNullableInt32(Schema.FIETSTERUG);
        VOETGANGER = new DbaseNullableInt32(Schema.VOETGANGER);
        CREATIE = new DbaseDateTime(Schema.CREATIE);
        VERSIE = new DbaseDateTime(Schema.VERSIE);

        Values =
        [
            WS_TEMPID,
            WS_OIDN,
            STATUS,
            MORF,
            WEGCAT,
            LSTRNMID,
            RSTRNMID,
            LBEHEER,
            RBEHEER,
            TOEGANG,
            VERHARDING,
            AUTOHEEN,
            AUTOTERUG,
            FIETSHEEN,
            FIETSTERUG,
            VOETGANGER,
            CREATIE,
            VERSIE
        ];
    }

    public DbaseInt32 WS_TEMPID { get; }
    public DbaseInt32 WS_OIDN { get; }
    public DbaseInt32 STATUS { get; }
    public DbaseInt32 MORF { get; }
    public DbaseString WEGCAT { get; }
    public DbaseNullableInt32 LSTRNMID { get; }
    public DbaseNullableInt32 RSTRNMID { get; }
    public DbaseString LBEHEER { get; }
    public DbaseString RBEHEER { get; }
    public DbaseInt32 TOEGANG { get; }
    public DbaseInt32 VERHARDING { get; }
    public DbaseNullableInt32 AUTOHEEN { get; }
    public DbaseNullableInt32 AUTOTERUG { get; }
    public DbaseNullableInt32 FIETSHEEN { get; }
    public DbaseNullableInt32 FIETSTERUG { get; }
    public DbaseNullableInt32 VOETGANGER { get; }
    public DbaseDateTime CREATIE { get; }
    public DbaseDateTime VERSIE { get; }
}
