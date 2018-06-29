namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentEuropeanRoadAttributeDbaseSchema : DbaseSchema
    {
        public RoadSegmentEuropeanRoadAttributeDbaseSchema()
        {
            EU_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(EU_OIDN)),
                new DbaseFieldLength(15));

            WS_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(WS_OIDN)),
                new DbaseFieldLength(15));

            EUNUMMER = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(EUNUMMER)),
                new DbaseFieldLength(4));

            BEGINTIJD = DbaseField.CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD)));

            BEGINORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(BEGINORG)),
                new DbaseFieldLength(18));

            LBLBGNORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLBGNORG)),
                new DbaseFieldLength(64));

            Fields = new DbaseField[]
            {
                EU_OIDN, WS_OIDN, EUNUMMER, BEGINTIJD, BEGINORG, LBLBGNORG
            };
        }

        public DbaseField EU_OIDN { get; }
        public DbaseField WS_OIDN { get; }
        public DbaseField EUNUMMER { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBGNORG { get; }
    }
}
