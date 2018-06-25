namespace RoadRegistry.Projections
{
    using Events;
    using Shaperon;

    public class RoadSegmentMorphologyRecord : DbaseRecord
    {
        public static readonly RoadSegmentMorphologySchema Schema = new RoadSegmentMorphologySchema();
        public static readonly RoadSegmentMorphologyTranslator Translator = new RoadSegmentMorphologyTranslator();

        public RoadSegmentMorphologyRecord(RoadSegmentMorphology value)
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