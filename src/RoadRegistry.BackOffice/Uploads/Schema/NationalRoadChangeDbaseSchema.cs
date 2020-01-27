namespace RoadRegistry.BackOffice.Uploads.Schema
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
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(TRANSACTID)),
                        new DbaseFieldLength(4)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(RECORDTYPE)),
                        new DbaseFieldLength(4))
            };
        }

        public DbaseField NW_OIDN => Fields[0];

        public DbaseField WS_OIDN => Fields[1];

        public DbaseField IDENT2 => Fields[2];

        public DbaseField TRANSACTID => Fields[3];

        public DbaseField RECORDTYPE => Fields[4];
    }
}
