namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class LaneDirectionDbaseSchema: DbaseSchema
    {
        public LaneDirectionDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateNumberField(
                    new DbaseFieldName(nameof(RICHTING)),
                    new DbaseFieldLength(2)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(LBLRICHT)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(DEFRICHT)),
                        new DbaseFieldLength(254))
            };
        }

        public DbaseField RICHTING => Fields[0];
        public DbaseField LBLRICHT => Fields[1];
        public DbaseField DEFRICHT => Fields[2];
    }
}
