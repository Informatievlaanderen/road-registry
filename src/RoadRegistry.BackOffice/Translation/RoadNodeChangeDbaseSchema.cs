namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeChangeDbaseSchema : DbaseSchema
    {
        public RoadNodeChangeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName("wegknoopID"),
                    new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName("type"),
                        new DbaseFieldLength(3)),

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

        public DbaseField WEGKNOOPID => Fields[0];

        public DbaseField TYPE => Fields[1];

        public DbaseField TransactID => Fields[2];

        public DbaseField RecordType => Fields[3];
    }
}
