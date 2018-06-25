namespace RoadRegistry.Projections
{
    using Events;
    using Shaperon;

    public class GradeSeparatedJunctionTypeRecord : DbaseRecord
    {
        private static readonly GradeSeparatedJunctionTypeSchema Schema = new GradeSeparatedJunctionTypeSchema();
        private static readonly GradeSeparatedJunctionTypeTranslator Translator = new GradeSeparatedJunctionTypeTranslator();

        public GradeSeparatedJunctionTypeRecord(GradeSeparatedJunctionType value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.TYPE, Translator.TranslateToIdentifier(value)),
                new DbaseString(Schema.LBLTYPE, Translator.TranslateToDutchName(value)),
                new DbaseString(Schema.DEFTYPE, Translator.TranslateToDutchDescription(value)),
            };
        }
    }
}
