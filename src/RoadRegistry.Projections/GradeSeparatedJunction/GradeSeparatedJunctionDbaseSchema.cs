namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;

    public class GradeSeparatedJunctionDbaseSchema : DbaseSchema
    {
        public GradeSeparatedJunctionDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(OK_OIDN)),
                    new DbaseFieldLength(15)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(TYPE)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLTYPE)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(BO_WS_OIDN)),
                        new DbaseFieldLength(15)),
                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(ON_WS_OIDN)),
                        new DbaseFieldLength(15)),

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

        public DbaseField OK_OIDN => Fields[0];
        public DbaseField TYPE => Fields[1];
        public DbaseField LBLTYPE => Fields[2];
        public DbaseField BO_WS_OIDN => Fields[3];
        public DbaseField ON_WS_OIDN => Fields[4];
        public DbaseField BEGINTIJD => Fields[5];
        public DbaseField BEGINORG => Fields[6];
        public DbaseField LBLBGNORG => Fields[7];
    }
}
