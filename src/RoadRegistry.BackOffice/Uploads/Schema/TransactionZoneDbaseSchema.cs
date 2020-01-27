namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class TransactionZoneDbaseSchema : DbaseSchema
    {
        public TransactionZoneDbaseSchema()
        {
            Fields = new []
            {
                DbaseField.CreateInt32Field(new DbaseFieldName(nameof(SOURCE_ID)),
                    new DbaseFieldLength(4)),
                DbaseField.CreateInt32Field(new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(4)),
                DbaseField.CreateStringField(new DbaseFieldName(nameof(BESCHRIJV)),
                    new DbaseFieldLength(254)),
                DbaseField.CreateStringField(new DbaseFieldName(nameof(OPERATOR)),
                    new DbaseFieldLength(254)),
                DbaseField.CreateStringField(new DbaseFieldName(nameof(ORG)),
                    new DbaseFieldLength(18)),
                DbaseField.CreateStringField(new DbaseFieldName(nameof(APPLICATIE)),
                    new DbaseFieldLength(18))
            };
        }

        public DbaseField SOURCE_ID => Fields[0];
        public DbaseField TYPE => Fields[1];
        public DbaseField BESCHRIJV => Fields[2];
        public DbaseField OPERATOR => Fields[3];
        public DbaseField ORG => Fields[4];
        public DbaseField APPLICATIE => Fields[5];

    }
}
