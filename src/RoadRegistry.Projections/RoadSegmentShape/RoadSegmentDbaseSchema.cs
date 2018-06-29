namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentDbaseSchema : DbaseSchema
    {
        public RoadSegmentDbaseSchema()
        {
            WS_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(WS_OIDN)),
                new DbaseFieldLength(15));

            WS_UIDN = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(WS_UIDN)),
                new DbaseFieldLength(18));

            WS_GIDN = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(WS_GIDN)),
                new DbaseFieldLength(18));

            B_WK_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(B_WK_OIDN)),
                new DbaseFieldLength(15));

            E_WK_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(E_WK_OIDN)),
                new DbaseFieldLength(15));

            STATUS = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(STATUS)),
                new DbaseFieldLength(2));

            LBLSTATUS = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLSTATUS)),
                new DbaseFieldLength(64));

            MORF = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(MORF)),
                new DbaseFieldLength(3));

            LBLMORF = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLMORF)),
                new DbaseFieldLength(64));

            WEGCAT = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(WEGCAT)),
                new DbaseFieldLength(5));

            LBLWEGCAT = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLWEGCAT)),
                new DbaseFieldLength(64));

            LSTRNMID = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(LSTRNMID)),
                new DbaseFieldLength(15));

            LSTRNM = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LSTRNM)),
                new DbaseFieldLength(80));

            RSTRNMID = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(RSTRNMID)),
                new DbaseFieldLength(15));

            RSTRNM = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(RSTRNM)),
                new DbaseFieldLength(80));

            BEHEER = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(BEHEER)),
                new DbaseFieldLength(18));

            LBLBEHEER = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLBEHEER)),
                new DbaseFieldLength(64));

            METHODE = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(METHODE)),
                new DbaseFieldLength(2));

            LBLMETHOD = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLMETHOD)),
                new DbaseFieldLength(64));

            OPNDATUM = DbaseField.CreateDateTimeField(new DbaseFieldName(nameof(OPNDATUM)));

            BEGINTIJD = DbaseField.CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD)));

            BEGINORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(BEGINORG)),
                new DbaseFieldLength(18));

            LBLBGNORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLBGNORG)),
                new DbaseFieldLength(64));

            TGBEP = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(TGBEP)),
                new DbaseFieldLength(2));

            LBLTGBEP = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLTGBEP)),
                new DbaseFieldLength(64));
            
            Fields = new DbaseField[]
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

        public DbaseField WS_OIDN { get; }
        public DbaseField WS_UIDN { get; }
        public DbaseField WS_GIDN { get; }
        public DbaseField B_WK_OIDN { get; }
        public DbaseField E_WK_OIDN { get; }
        public DbaseField STATUS { get; }
        public DbaseField LBLSTATUS { get; }
        public DbaseField MORF { get; }
        public DbaseField LBLMORF { get; }
        public DbaseField WEGCAT { get; }
        public DbaseField LBLWEGCAT { get; }
        public DbaseField LSTRNMID { get; }
        public DbaseField LSTRNM { get; }
        public DbaseField RSTRNMID { get; }
        public DbaseField RSTRNM { get; }
        public DbaseField BEHEER { get; }
        public DbaseField LBLBEHEER { get; }
        public DbaseField METHODE { get; }
        public DbaseField LBLMETHOD { get; }
        public DbaseField OPNDATUM { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBGNORG { get; }
        public DbaseField TGBEP { get; }
        public DbaseField LBLTGBEP { get; }

    }
}
