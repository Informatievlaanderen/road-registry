namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentLaneAttributeDbaseSchema : DbaseSchema
    {
        public RoadSegmentLaneAttributeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(RS_OIDN)),
                    new DbaseFieldLength(15)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(15)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(WS_GIDN)),
                        new DbaseFieldLength(18)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(AANTAL)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(RICHTING)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLRICHT)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateDoubleField(
                        new DbaseFieldName(nameof(VANPOS)),
                        new DbaseFieldLength(9),
                        new DbaseDecimalCount(3)),

                DbaseField
                    .CreateDoubleField(
                        new DbaseFieldName(nameof(TOTPOS)),
                        new DbaseFieldLength(9),
                        new DbaseDecimalCount(3)),

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

        public DbaseField RS_OIDN => Fields[0];
        public DbaseField WS_OIDN => Fields[1];
        public DbaseField WS_GIDN => Fields[2];
        public DbaseField AANTAL => Fields[3];
        public DbaseField RICHTING => Fields[4];
        public DbaseField LBLRICHT => Fields[5];
        public DbaseField VANPOS => Fields[6];
        public DbaseField TOTPOS => Fields[7];
        public DbaseField BEGINTIJD => Fields[8];
        public DbaseField BEGINORG => Fields[9];
        public DbaseField LBLBGNORG => Fields[10];
    }
}
