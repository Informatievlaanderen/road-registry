namespace RoadRegistry.BackOffice.Schema.RoadNodes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadNodeDbaseSchema : DbaseSchema
    {
        public RoadNodeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateNumberField(
                    new DbaseFieldName(nameof(WK_OIDN)),
                    new DbaseFieldLength(15)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(WK_UIDN)),
                        new DbaseFieldLength(18)),

                DbaseField
                    .CreateNumberField(
                        new DbaseFieldName(nameof(TYPE)),
                        new DbaseFieldLength(2)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(LBLTYPE)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateDateTimeField(new DbaseFieldName(nameof(BEGINTIJD))),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(BEGINORG)),
                        new DbaseFieldLength(18)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(LBLBGNORG)),
                        new DbaseFieldLength(64))
            };
        }

        public DbaseField WK_OIDN => Fields[0];
        public DbaseField WK_UIDN => Fields[1];
        public DbaseField TYPE => Fields[2];
        public DbaseField LBLTYPE => Fields[3];
        public DbaseField BEGINTIJD => Fields[4];
        public DbaseField BEGINORG => Fields[5];
        public DbaseField LBLBGNORG => Fields[6];
    }
}
