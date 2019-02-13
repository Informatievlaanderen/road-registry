namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GradeSeparatedJunctionChangeDbaseSchema : DbaseSchema
    {
        public GradeSeparatedJunctionChangeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(OK_OIDN)),
                    new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(TYPE)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(BO_WS_OIDN)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(ON_WS_OIDN)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName("transactID"),
                        new DbaseFieldLength(4)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName("recordtype"),
                        new DbaseFieldLength(4))
            };
        }

        public DbaseField OK_OIDN => Fields[0];

        public DbaseField TYPE => Fields[1];

        public DbaseField BO_WS_OIDN => Fields[2];

        public DbaseField ON_WS_OIDN => Fields[3];

        public DbaseField TransactID => Fields[4];

        public DbaseField RecordType => Fields[5];
    }
}