namespace RoadRegistry.BackOffice.Translation
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentChangeDbaseSchema : DbaseSchema
    {
        public RoadSegmentChangeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(METHODE)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(BEHEERDER)),
                        new DbaseFieldLength(18)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(MORFOLOGIE)),
                        new DbaseFieldLength(3)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(STATUS)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(WEGCAT)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(B_WK_OIDN)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(E_WK_OIDN)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(LSTRNMID)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(RSTRNMID)),
                        new DbaseFieldLength(10)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(TGBEP)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(TRANSACTID)),
                        new DbaseFieldLength(4)),

                DbaseField
                    .CreateInt16Field(
                        new DbaseFieldName(nameof(RECORDTYPE)),
                        new DbaseFieldLength(4)),

                DbaseField
                    .CreateInt32Field(
                        new DbaseFieldName(nameof(EVENTIDN)),
                        new DbaseFieldLength(10))
            };
        }

        public DbaseField WS_OIDN => Fields[0];

        public DbaseField METHODE => Fields[1];

        public DbaseField BEHEERDER => Fields[2];

        public DbaseField MORFOLOGIE => Fields[3];

        public DbaseField STATUS => Fields[4];

        public DbaseField WEGCAT => Fields[5];

        public DbaseField B_WK_OIDN => Fields[6];

        public DbaseField E_WK_OIDN => Fields[7];

        public DbaseField LSTRNMID => Fields[8];

        public DbaseField RSTRNMID => Fields[9];

        public DbaseField TGBEP => Fields[10];

        public DbaseField TRANSACTID => Fields[11];

        public DbaseField RECORDTYPE => Fields[12];

        public DbaseField EVENTIDN => Fields[13];
    }
}
