namespace RoadRegistry.Projections.RoadSegmentDynamicHardeningAttribute
{
    using Shaperon;

    public class RoadSegmentDynamicHardeningAttributeDbaseSchema
    {
        public RoadSegmentDynamicHardeningAttributeDbaseSchema()
        {
            WV_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(WV_OIDN)),
                ByteOffset.Initial,
                new DbaseFieldLength(15));

            WS_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(WS_OIDN)),
                ByteOffset.Initial,
                new DbaseFieldLength(15));

            WS_GIDN = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(WS_GIDN)),
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

            VANPOS = DbaseField.CreateDoubleField(
                new DbaseFieldName(nameof(VANPOS)),
                ByteOffset.Initial,
                new DbaseFieldLength(9),
                new DbaseDecimalCount(3));

            TOTPOS = DbaseField.CreateDoubleField(
                new DbaseFieldName(nameof(TOTPOS)),
                ByteOffset.Initial,
                new DbaseFieldLength(9),
                new DbaseDecimalCount(3));

            BEGINTIJD = DbaseField.CreateDateTimeField(
                new DbaseFieldName(nameof(BEGINTIJD)),
                ByteOffset.Initial);

            BEGINORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(BEGINORG)),
                ByteOffset.Initial,
                new DbaseFieldLength(18));

            LBLBGNORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLBGNORG)),
                ByteOffset.Initial,
                new DbaseFieldLength(64));

        }

        public DbaseField WV_OIDN { get; set; }
        public DbaseField WS_OIDN { get; set; }
        public DbaseField WS_GIDN { get; set; }
        public DbaseField TYPE { get; set; }
        public DbaseField LBLTYPE { get; set; }
        public DbaseField VANPOS { get; set; }
        public DbaseField TOTPOS { get; set; }
        public DbaseField BEGINTIJD { get; set; }
        public DbaseField BEGINORG { get; set; }
        public DbaseField LBLBGNORG { get; set; }
    }
}
