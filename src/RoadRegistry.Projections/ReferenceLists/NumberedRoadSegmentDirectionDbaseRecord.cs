namespace RoadRegistry.Projections
{
    using Events;
    using Aiv.Vbr.Shaperon;

    public class NumberedRoadSegmentDirectionDbaseRecord : DbaseRecord
    {
        private static readonly NumberedRoadSegmentDirectionDbaseSchema Schema = new NumberedRoadSegmentDirectionDbaseSchema();
        private static readonly NumberedRoadSegmentDirectionTranslator Translator = new NumberedRoadSegmentDirectionTranslator();

        public NumberedRoadSegmentDirectionDbaseRecord(NumberedRoadSegmentDirection value)
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