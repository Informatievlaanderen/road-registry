namespace RoadRegistry.Projections
{
    using Events;
    using Shaperon;

    public class LaneDirectionRecord : DbaseRecord
    {
        private static readonly LaneDirectionSchema Schema = new LaneDirectionSchema();
        private static readonly LaneDirectionTranslator Translator = new LaneDirectionTranslator();

        public LaneDirectionRecord(LaneDirection value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.RICHTING, Translator.TranslateToIdentifier(value)),
                new DbaseString(Schema.LBLRICHT, Translator.TranslateToDutchName(value)),
                new DbaseString(Schema.DEFRICHT, Translator.TranslateToDutchDescription(value)),
            };
        }
    }
}
