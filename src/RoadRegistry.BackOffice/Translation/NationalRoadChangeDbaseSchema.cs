namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NationalRoadChangeDbaseSchema : DbaseSchema
    {
        public NationalRoadChangeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(NW_OIDN)),
                    new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(IDENT2)),
                        new DbaseFieldLength(8)),

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

        public DbaseField NW_OIDN => Fields[0];

        public DbaseField WS_OIDN => Fields[1];

        public DbaseField IDENT2 => Fields[2];

        public DbaseField TransactID => Fields[3];

        public DbaseField RecordType => Fields[4];
    }
}
