namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentDynamicWidthAttributeDbaseSchema
    {
        public RoadSegmentDynamicWidthAttributeDbaseSchema()
        {
            WB_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(WB_OIDN)),
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

            BREEDTE = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(BREEDTE)),
                ByteOffset.Initial,
                new DbaseFieldLength(2));

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

        public DbaseField WB_OIDN { get; set; }
        public DbaseField WS_OIDN { get; set; }
        public DbaseField WS_GIDN { get; set; }
        public DbaseField BREEDTE { get; set; }
        public DbaseField VANPOS { get; set; }
        public DbaseField TOTPOS { get; set; }
        public DbaseField BEGINTIJD { get; set; }
        public DbaseField BEGINORG { get; set; }
        public DbaseField LBLBGNORG { get; set; }
    }
}
