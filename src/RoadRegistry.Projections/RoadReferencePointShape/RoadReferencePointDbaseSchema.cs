namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadReferencePointDbaseSchema
    {
        public RoadReferencePointDbaseSchema()
        {
            RP_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(RP_OIDN)),
                ByteOffset.Initial,
                new DbaseFieldLength(15));

            RP_UIDN = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(RP_UIDN)),
                ByteOffset.Initial,
                new DbaseFieldLength(18));

            IDENT8 =  DbaseField.CreateStringField(
                new DbaseFieldName(nameof(IDENT8)),
                ByteOffset.Initial,
                new DbaseFieldLength(8));

            OPSCHRIFT = DbaseField.CreateDoubleField(
                new DbaseFieldName(nameof(OPSCHRIFT)),
                ByteOffset.Initial,
                new DbaseFieldLength(5),
                new DbaseDecimalCount(1));

            TYPE = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(TYPE)),
                ByteOffset.Initial,
                new DbaseFieldLength(2));

            LBLTYPE = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLTYPE)),
                ByteOffset.Initial,
                new DbaseFieldLength(64));

            BEGINTIJD = DbaseField.CreateDateTimeField(
                new DbaseFieldName(nameof(BEGINTIJD)),
                ByteOffset.Initial);

            BEGINORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(BEGINORG)),
                ByteOffset.Initial,
                new DbaseFieldLength(18));

            LBLBEGINORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLBEGINORG)),
                ByteOffset.Initial,
                new DbaseFieldLength(64));
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
