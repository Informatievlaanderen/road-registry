namespace RoadRegistry.Editor.Schema.Lists
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GradeSeparatedJunctionTypeDbaseSchema : DbaseSchema
    {
        public GradeSeparatedJunctionTypeDbaseSchema()
        {
            Fields = new[]
            {
                DbaseField.CreateNumberField(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(LBLTYPE)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(DEFTYPE)),
                        new DbaseFieldLength(254))
            };
        }

        public DbaseField TYPE => Fields[0];
        public DbaseField LBLTYPE => Fields[1];
        public DbaseField DEFTYPE => Fields[2];
    }
}
