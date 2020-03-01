namespace RoadRegistry.BackOffice.Schema.RoadSegments
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentDbaseSchema Schema = new RoadSegmentDbaseSchema();

        public RoadSegmentDbaseRecord()
        {
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            WS_UIDN = new DbaseCharacter(Schema.WS_UIDN);
            WS_GIDN = new DbaseCharacter(Schema.WS_GIDN);
            B_WK_OIDN = new DbaseNumber(Schema.B_WK_OIDN);
            E_WK_OIDN = new DbaseNumber(Schema.E_WK_OIDN);
            STATUS = new DbaseNumber(Schema.STATUS);
            LBLSTATUS = new DbaseCharacter(Schema.LBLSTATUS);
            MORF = new DbaseNumber(Schema.MORF);
            LBLMORF = new DbaseCharacter(Schema.LBLMORF);
            WEGCAT = new DbaseCharacter(Schema.WEGCAT);
            LBLWEGCAT = new DbaseCharacter(Schema.LBLWEGCAT);
            LSTRNMID = new DbaseNumber(Schema.LSTRNMID);
            LSTRNM = new DbaseCharacter(Schema.LSTRNM);
            RSTRNMID = new DbaseNumber(Schema.RSTRNMID);
            RSTRNM = new DbaseCharacter(Schema.RSTRNM);
            BEHEER = new DbaseCharacter(Schema.BEHEER);
            LBLBEHEER = new DbaseCharacter(Schema.LBLBEHEER);
            METHODE = new DbaseNumber(Schema.METHODE);
            LBLMETHOD = new DbaseCharacter(Schema.LBLMETHOD);
            OPNDATUM = new DbaseDateTime(Schema.OPNDATUM);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseCharacter(Schema.BEGINORG);
            LBLBGNORG = new DbaseCharacter(Schema.LBLBGNORG);
            TGBEP = new DbaseNumber(Schema.TGBEP);
            LBLTGBEP = new DbaseCharacter(Schema.LBLTGBEP);

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
                LBLTGBEP,
            };
        }

        public DbaseNumber WS_OIDN { get; }
        public DbaseCharacter WS_UIDN { get; }
        public DbaseCharacter WS_GIDN { get; }
        public DbaseNumber B_WK_OIDN { get; }
        public DbaseNumber E_WK_OIDN { get; }
        public DbaseNumber STATUS { get; }
        public DbaseCharacter LBLSTATUS { get; }
        public DbaseNumber MORF { get; }
        public DbaseCharacter LBLMORF { get; }
        public DbaseCharacter WEGCAT { get; }
        public DbaseCharacter LBLWEGCAT { get; }
        public DbaseNumber LSTRNMID { get; }
        public DbaseCharacter LSTRNM { get; }
        public DbaseNumber RSTRNMID { get; }
        public DbaseCharacter RSTRNM { get; }
        public DbaseCharacter BEHEER { get; }
        public DbaseCharacter LBLBEHEER { get; }
        public DbaseNumber METHODE { get; }
        public DbaseCharacter LBLMETHOD { get; }
        public DbaseDateTime OPNDATUM { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseCharacter BEGINORG { get; }
        public DbaseCharacter LBLBGNORG { get; }
        public DbaseNumber TGBEP { get; }
        public DbaseCharacter LBLTGBEP { get; }
    }
}
