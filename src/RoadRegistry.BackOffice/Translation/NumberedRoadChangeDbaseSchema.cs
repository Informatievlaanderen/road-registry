namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NumberedRoadChangeDbaseSchema : DbaseSchema
    {
        public NumberedRoadChangeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(GW_OIDN)),
                    new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(IDENT8)),
                        new DbaseFieldLength(8)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(RICHTING)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(VOLGNUMMER)),
                        new DbaseFieldLength(5)),

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

        public DbaseField GW_OIDN => Fields[0];

        public DbaseField WS_OIDN => Fields[1];

        public DbaseField IDENT8 => Fields[2];

        public DbaseField RICHTING => Fields[3];

        public DbaseField VOLGNUMMER => Fields[4];

        public DbaseField TransactID => Fields[5];

        public DbaseField RecordType => Fields[6];
    }
}
