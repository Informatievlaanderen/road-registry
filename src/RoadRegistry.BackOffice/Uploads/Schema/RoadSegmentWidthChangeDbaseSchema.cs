namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentWidthChangeDbaseSchema : DbaseSchema
    {
        public RoadSegmentWidthChangeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(WB_OIDN)),
                    new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateDoubleField(
                        new DbaseFieldName(nameof(VANPOSITIE)),
                        new DbaseFieldLength(7),
                        new DbaseDecimalCount(3)),

                DbaseField
                    .CreateDoubleField(
                        new DbaseFieldName(nameof(TOTPOSITIE)),
                        new DbaseFieldLength(7),
                        new DbaseDecimalCount(3)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(BREEDTE)),
                        new DbaseFieldLength(4)),

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

        public DbaseField WB_OIDN => Fields[0];

        public DbaseField WS_OIDN => Fields[1];

        public DbaseField VANPOSITIE => Fields[2];

        public DbaseField TOTPOSITIE => Fields[3];

        public DbaseField BREEDTE => Fields[4];

        public DbaseField TRANSACTID => Fields[5];

        public DbaseField RECORDTYPE => Fields[6];
    }
}
