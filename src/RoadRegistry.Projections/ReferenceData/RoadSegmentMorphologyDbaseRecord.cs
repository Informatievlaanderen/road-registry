namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;
    using Messages;

    public class RoadSegmentMorphologyDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentMorphologyDbaseSchema Schema = new RoadSegmentMorphologyDbaseSchema();
        public static readonly RoadSegmentMorphologyTranslator Translator = new RoadSegmentMorphologyTranslator();

        public RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.MORF, Translator.TranslateToIdentifier(value)),
                new DbaseString(Schema.LBLMORF, Translator.TranslateToDutchName(value)),
                new DbaseString(Schema.DEFMORF, Translator.TranslateToDutchDescription(value)),
            };
        }
    }
}
