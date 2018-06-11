namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentDbaseSchema
    {
        public RoadSegmentDbaseSchema()
        {
            WS_OIDN = new DbaseField(
                new DbaseFieldName(nameof(WS_OIDN)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            WS_UIDN = new DbaseField(
                new DbaseFieldName(nameof(WS_UIDN)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(18),
                new DbaseDecimalCount(0));

            WS_GIDN = new DbaseField(
                new DbaseFieldName(nameof(WS_GIDN)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(18),
                new DbaseDecimalCount(0));

            B_WK_OIDN = new DbaseField(
                new DbaseFieldName(nameof(B_WK_OIDN)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            E_WK_OIDN = new DbaseField(
                new DbaseFieldName(nameof(E_WK_OIDN)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            STATUS = new DbaseField(
                new DbaseFieldName(nameof(STATUS)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(2),
                new DbaseDecimalCount(0));

            LBLSTATUS = new DbaseField(
                new DbaseFieldName(nameof(LBLSTATUS)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
                new DbaseDecimalCount(0));

            MORF = new DbaseField(
                new DbaseFieldName(nameof(MORF)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(3),
                new DbaseDecimalCount(0));

            LBLMORF = new DbaseField(
                new DbaseFieldName(nameof(LBLMORF)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
                new DbaseDecimalCount(0));

            WEGCAT = new DbaseField(
                new DbaseFieldName(nameof(WEGCAT)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(5),
                new DbaseDecimalCount(0));

            LBLWEGCAT = new DbaseField(
                new DbaseFieldName(nameof(LBLWEGCAT)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
                new DbaseDecimalCount(0));

            LSTRNMID = new DbaseField(
                new DbaseFieldName(nameof(LSTRNMID)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            LSTRNM = new DbaseField(
                new DbaseFieldName(nameof(LSTRNM)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(80),
                new DbaseDecimalCount(0));

            RSTRNMID = new DbaseField(
                new DbaseFieldName(nameof(RSTRNMID)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            RSTRNM = new DbaseField(
                new DbaseFieldName(nameof(RSTRNM)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(80),
                new DbaseDecimalCount(0));

            BEHEER = new DbaseField(
                new DbaseFieldName(nameof(BEHEER)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(18),
                new DbaseDecimalCount(0));

            LBLBEHEER = new DbaseField(
                new DbaseFieldName(nameof(LBLBEHEER)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
                new DbaseDecimalCount(0));

            METHODE = new DbaseField(
                new DbaseFieldName(nameof(METHODE)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(2),
                new DbaseDecimalCount(0));

            LBLMETHOD = new DbaseField(
                new DbaseFieldName(nameof(LBLMETHOD)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
                new DbaseDecimalCount(0));

            OPNDATUM = new DbaseField(
                new DbaseFieldName(nameof(OPNDATUM)),
                DbaseFieldType.DateTime,
                ByteOffset.Initial,
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            BEGINTIJD = new DbaseField(
                new DbaseFieldName(nameof(BEGINTIJD)),
                DbaseFieldType.DateTime,
                ByteOffset.Initial,
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            BEGINORG = new DbaseField(
                new DbaseFieldName(nameof(BEGINORG)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(18),
                new DbaseDecimalCount(0));

            LBLBGNORG = new DbaseField(
                new DbaseFieldName(nameof(LBLBGNORG)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
                new DbaseDecimalCount(0));

            TGBEP = new DbaseField(
                new DbaseFieldName(nameof(TGBEP)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(2),
                new DbaseDecimalCount(0));

            LBLTGBEP = new DbaseField(
                new DbaseFieldName(nameof(LBLTGBEP)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
                new DbaseDecimalCount(0));
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
