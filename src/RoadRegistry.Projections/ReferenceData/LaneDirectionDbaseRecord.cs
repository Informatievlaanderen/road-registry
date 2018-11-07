namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;
    using Messages;

    public class LaneDirectionDbaseRecord : DbaseRecord
    {
        private static readonly LaneDirectionDbaseSchema Schema = new LaneDirectionDbaseSchema();
        private static readonly LaneDirectionTranslator Translator = new LaneDirectionTranslator();

        public LaneDirectionDbaseRecord(LaneDirection value)
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
