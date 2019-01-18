namespace RoadRegistry.BackOffice.Schema.RoadNodes
{
    using Aiv.Vbr.Shaperon;

    public class RoadNodeDbaseSchema : DbaseSchema
    {
        public RoadNodeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(WK_OIDN)),
                    new DbaseFieldLength(15)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(WK_UIDN)),
                        new DbaseFieldLength(18)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(TYPE)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLTYPE)),
                        new DbaseFieldLength(64)),

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

        public DbaseField WK_OIDN => Fields[0];
        public DbaseField WK_UIDN => Fields[1];
        public DbaseField TYPE => Fields[2];
        public DbaseField LBLTYPE => Fields[3];
        public DbaseField BEGINTIJD => Fields[4];
        public DbaseField BEGINORG => Fields[5];
        public DbaseField LBLBGNORG => Fields[6];
    }
}
