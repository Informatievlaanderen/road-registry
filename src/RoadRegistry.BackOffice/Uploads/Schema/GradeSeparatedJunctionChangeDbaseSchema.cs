namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GradeSeparatedJunctionChangeDbaseSchema : DbaseSchema
    {
        public GradeSeparatedJunctionChangeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateNumberField(
                    new DbaseFieldName(nameof(OK_OIDN)),
                    new DbaseFieldLength(10),
                    new DbaseDecimalCount(0)),

                DbaseField
                    .CreateNumberField(
                        new DbaseFieldName(nameof(TYPE)),
                        new DbaseFieldLength(2),
                        new DbaseDecimalCount(0)),

                DbaseField
                    .CreateNumberField(
                        new DbaseFieldName(nameof(BO_WS_OIDN)),
                        new DbaseFieldLength(10),
                        new DbaseDecimalCount(0)),

                DbaseField
                    .CreateNumberField(
                        new DbaseFieldName(nameof(ON_WS_OIDN)),
                        new DbaseFieldLength(10),
                        new DbaseDecimalCount(0)),

                DbaseField
                    .CreateNumberField(
                        new DbaseFieldName(nameof(TRANSACTID)),
                        new DbaseFieldLength(4),
                        new DbaseDecimalCount(0)),

                DbaseField
                    .CreateNumberField(
                        new DbaseFieldName(nameof(RECORDTYPE)),
                        new DbaseFieldLength(4),
                        new DbaseDecimalCount(0))
            };
        }

        public DbaseField OK_OIDN => Fields[0];

        public DbaseField TYPE => Fields[1];

        public DbaseField BO_WS_OIDN => Fields[2];

        public DbaseField ON_WS_OIDN => Fields[3];

        public DbaseField TRANSACTID => Fields[4];

        public DbaseField RECORDTYPE => Fields[5];
    }
}
