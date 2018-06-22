namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentDynamicLaneAttributeDbaseSchema
    {
        public RoadSegmentDynamicLaneAttributeDbaseSchema()
        {
            RS_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(RS_OIDN)),
                new DbaseFieldLength(15));

            WS_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(WS_OIDN)),
                new DbaseFieldLength(15));

            WS_GIDN = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(WS_GIDN)),
                new DbaseFieldLength(18));

            AANTAL = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(AANTAL)),
                new DbaseFieldLength(2));

            RICHTING = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(RICHTING)),
                new DbaseFieldLength(2));

            LBLRICHT = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLRICHT)),
                new DbaseFieldLength(64));

            VANPOS = DbaseField.CreateDoubleField(
                new DbaseFieldName(nameof(VANPOS)),
                new DbaseFieldLength(9),
                new DbaseDecimalCount(3));

            TOTPOS = DbaseField.CreateDoubleField(
                new DbaseFieldName(nameof(TOTPOS)),
                new DbaseFieldLength(9),
                new DbaseDecimalCount(3));

            BEGINTIJD = DbaseField.CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD)));

            BEGINORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(BEGINORG)),
                new DbaseFieldLength(18));

            LBLBGNORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLBGNORG)),
                new DbaseFieldLength(64));
        }

        public DbaseField RS_OIDN { get; set; }
        public DbaseField WS_OIDN { get; set; }
        public DbaseField WS_GIDN { get; set; }
        public DbaseField AANTAL { get; set; }
        public DbaseField RICHTING { get; set; }
        public DbaseField LBLRICHT { get; set; }
        public DbaseField VANPOS { get; set; }
        public DbaseField TOTPOS { get; set; }
        public DbaseField BEGINTIJD { get; set; }
        public DbaseField BEGINORG { get; set; }
        public DbaseField LBLBGNORG { get; set; }
    }
}
