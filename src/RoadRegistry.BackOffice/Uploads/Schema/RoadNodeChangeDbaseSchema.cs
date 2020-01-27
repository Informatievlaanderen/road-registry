namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeChangeDbaseSchema : DbaseSchema
    {
        public RoadNodeChangeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(WEGKNOOPID)),
                    new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(TYPE)),
                        new DbaseFieldLength(3)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(TRANSACTID)),
                        new DbaseFieldLength(4)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(RECORDTYPE)),
                        new DbaseFieldLength(4))
            };
        }

        public DbaseField WEGKNOOPID => Fields[0];

        public DbaseField TYPE => Fields[1];

        public DbaseField TRANSACTID => Fields[2];

        public DbaseField RECORDTYPE => Fields[3];
    }
}
