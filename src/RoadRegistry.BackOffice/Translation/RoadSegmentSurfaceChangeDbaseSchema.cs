namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentSurfaceChangeDbaseSchema : DbaseSchema
    {
        public RoadSegmentSurfaceChangeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(WV_OIDN)),
                    new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateDoubleField(
                        new DbaseFieldName("vanPositie"),
                        new DbaseFieldLength(7),
                        new DbaseDecimalCount(3)),

                DbaseField
                    .CreateDoubleField(
                        new DbaseFieldName("totPositie"),
                        new DbaseFieldLength(7),
                        new DbaseDecimalCount(3)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName("type"),
                        new DbaseFieldLength(4)),

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

        public DbaseField WV_OIDN => Fields[0];

        public DbaseField WS_OIDN => Fields[1];

        public DbaseField VAN_POSITIE => Fields[2];

        public DbaseField TOT_POSITIE => Fields[3];

        public DbaseField TYPE => Fields[4];

        public DbaseField TransactID => Fields[5];

        public DbaseField RecordType => Fields[6];
    }
}