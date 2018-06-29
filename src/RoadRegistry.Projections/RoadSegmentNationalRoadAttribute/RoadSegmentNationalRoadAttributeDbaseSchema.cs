namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentNationalRoadAttributeDbaseSchema : DbaseSchema
    {
        public RoadSegmentNationalRoadAttributeDbaseSchema()
        {
            NW_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(NW_OIDN)),
                new DbaseFieldLength(15));

            WS_OIDN = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(WS_OIDN)),
                new DbaseFieldLength(15));

            IDENT2 = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(IDENT2)),
                new DbaseFieldLength(8));

            BEGINTIJD = DbaseField.CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD)));

            BEGINORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(BEGINORG)),
                new DbaseFieldLength(18));

            LBLBGNORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLBGNORG)),
                new DbaseFieldLength(64));
        
            Fields = new DbaseField[]
            {
                NW_OIDN, WS_OIDN, IDENT2, BEGINTIJD, BEGINORG, LBLBGNORG
            };
        }

        public DbaseField NW_OIDN { get; }
        public DbaseField WS_OIDN { get; }
        public DbaseField IDENT2 { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBGNORG { get; }
    }
}
