namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentNationalRoadAttributeDbaseSchema : DbaseSchema
    {
        public RoadSegmentNationalRoadAttributeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(NW_OIDN)),
                    new DbaseFieldLength(15)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(15)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(IDENT2)),
                        new DbaseFieldLength(8)),

                DbaseField
                    .CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD))),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(BEGINORG)),
                        new DbaseFieldLength(18)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLBGNORG)),
                        new DbaseFieldLength(64))
            };
        }

        public DbaseField NW_OIDN => Fields[0];
        public DbaseField WS_OIDN => Fields[1];
        public DbaseField IDENT2 => Fields[2];
        public DbaseField BEGINTIJD => Fields[3];
        public DbaseField BEGINORG => Fields[4];
        public DbaseField LBLBGNORG => Fields[5];
    }
}
