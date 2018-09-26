namespace RoadRegistry.Projections
{
    using Events;
    using Aiv.Vbr.Shaperon;

    public class GradeSeparatedJunctionTypeDbaseRecord : DbaseRecord
    {
        private static readonly GradeSeparatedJunctionTypeDbaseSchema Schema = new GradeSeparatedJunctionTypeDbaseSchema();
        private static readonly GradeSeparatedJunctionTypeTranslator Translator = new GradeSeparatedJunctionTypeTranslator();

        public GradeSeparatedJunctionTypeDbaseRecord(GradeSeparatedJunctionType value)
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
