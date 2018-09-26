namespace RoadRegistry.Projections
{
    using Events;
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentCategoryDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentCategoryDbaseSchema Schema = new RoadSegmentCategoryDbaseSchema();
        private static readonly RoadSegmentCategoryTranslator Translator = new RoadSegmentCategoryTranslator();

        public RoadSegmentCategoryDbaseRecord(RoadSegmentCategory value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseString(Schema.WEGCAT, Translator.TranslateToIdentifier(value)),
                new DbaseString(Schema.LBLWEGCAT, Translator.TranslateToDutchName(value)),
                new DbaseString(Schema.DEFWEGCAT, Translator.TranslateToDutchDescription(value)),
            };
        }
    }
}
