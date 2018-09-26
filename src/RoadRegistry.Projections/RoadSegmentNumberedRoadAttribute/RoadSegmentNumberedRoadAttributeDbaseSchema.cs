namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentNumberedRoadAttributeDbaseSchema : DbaseSchema
    {
        public RoadSegmentNumberedRoadAttributeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(GW_OIDN)),
                    new DbaseFieldLength(15)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(15)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(IDENT8)),
                        new DbaseFieldLength(8)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(RICHTING)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLRICHT)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(VOLGNUMMER)),
                        new DbaseFieldLength(5)),

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

        public DbaseField GW_OIDN => Fields[0];
        public DbaseField WS_OIDN => Fields[1];
        public DbaseField IDENT8 => Fields[2];
        public DbaseField RICHTING => Fields[3];
        public DbaseField LBLRICHT => Fields[4];
        public DbaseField VOLGNUMMER => Fields[5];
        public DbaseField BEGINTIJD => Fields[6];
        public DbaseField BEGINORG => Fields[7];
        public DbaseField LBLBGNORG => Fields[8];
    }
}
