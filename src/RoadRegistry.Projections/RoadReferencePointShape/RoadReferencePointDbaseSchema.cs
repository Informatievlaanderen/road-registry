namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadReferencePointDbaseSchema : DbaseSchema
    {
        public RoadReferencePointDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(RP_OIDN)),
                    new DbaseFieldLength(15)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(RP_UIDN)),
                        new DbaseFieldLength(18)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(IDENT8)),
                        new DbaseFieldLength(8)),

                DbaseField
                    .CreateDoubleField(
                        new DbaseFieldName(nameof(OPSCHRIFT)),
                        new DbaseFieldLength(5),
                        new DbaseDecimalCount(1)),

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

        public DbaseField RP_OIDN => Fields[0];
        public DbaseField RP_UIDN => Fields[1];
        public DbaseField IDENT8 => Fields[2];
        public DbaseField OPSCHRIFT => Fields[3];
        public DbaseField TYPE => Fields[4];
        public DbaseField LBLTYPE => Fields[5];
        public DbaseField BEGINTIJD => Fields[6];
        public DbaseField BEGINORG => Fields[7];
        public DbaseField LBLBGNORG => Fields[8];
    }
}
