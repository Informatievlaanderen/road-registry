namespace RoadRegistry.BackOffice.Schema.RoadSegmentEuropeanRoadAttributes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentEuropeanRoadAttributeDbaseSchema : DbaseSchema
    {
        public RoadSegmentEuropeanRoadAttributeDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateNumberField(
                    new DbaseFieldName(nameof(EU_OIDN)),
                    new DbaseFieldLength(15)),

                DbaseField
                    .CreateNumberField(
                        new DbaseFieldName(nameof(WS_OIDN)),
                        new DbaseFieldLength(15)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(EUNUMMER)),
                        new DbaseFieldLength(4)),

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

        public DbaseField EU_OIDN => Fields[0];
        public DbaseField WS_OIDN => Fields[1];
        public DbaseField EUNUMMER => Fields[2];
        public DbaseField BEGINTIJD => Fields[3];
        public DbaseField BEGINORG => Fields[4];
        public DbaseField LBLBGNORG => Fields[5];
    }
}
