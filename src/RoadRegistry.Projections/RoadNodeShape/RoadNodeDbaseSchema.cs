namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadNodeDbaseSchema
    {
        public RoadNodeDbaseSchema()
        {
            WK_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(WK_OIDN)),
                ByteOffset.Initial,
                new DbaseFieldLength(15));

            WK_UIDN = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(WK_UIDN)),
                ByteOffset.Initial,
                new DbaseFieldLength(18));

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

            LBLBGINORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLBGINORG)),
                ByteOffset.Initial,
                new DbaseFieldLength(64));
        }

        public DbaseField WK_OIDN { get; }
        public DbaseField WK_UIDN { get; }
        public DbaseField TYPE { get; }
        public DbaseField LBLTYPE { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBGINORG { get; }
    }
}
