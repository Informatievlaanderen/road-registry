namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadReferencePointDbaseSchema
    {
        public RoadReferencePointDbaseSchema()
        {
            RP_OIDN = new DbaseField(
                new DbaseFieldName(nameof(RP_OIDN)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            RP_UIDN = new DbaseField(
                new DbaseFieldName(nameof(RP_UIDN)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(18),
                new DbaseDecimalCount(0));

            IDENT8 = new DbaseField(
                new DbaseFieldName(nameof(IDENT8)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(8),
                new DbaseDecimalCount(0));

            OPSCHRIFT = new DbaseField(
                new DbaseFieldName(nameof(OPSCHRIFT)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(5),
                new DbaseDecimalCount(1));

            TYPE = new DbaseField(
                new DbaseFieldName(nameof(TYPE)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(2),
                new DbaseDecimalCount(0));

            LBLTYPE = new DbaseField(
                new DbaseFieldName(nameof(LBLTYPE)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
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

            LBLBEGINORG = new DbaseField(
                new DbaseFieldName(nameof(LBLBEGINORG)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
                new DbaseDecimalCount(0));
        }

        public DbaseField RP_UIDN { get; }
        public DbaseField RP_OIDN { get; }
        public DbaseField IDENT8 { get; }
        public DbaseField OPSCHRIFT { get; }
        public DbaseField TYPE { get; }
        public DbaseField LBLTYPE { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBEGINORG { get; }
    }
}
