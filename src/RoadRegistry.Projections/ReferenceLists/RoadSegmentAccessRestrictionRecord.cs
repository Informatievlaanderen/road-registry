namespace RoadRegistry.Projections
{
    using Events;
    using Shaperon;

    public class RoadSegmentAccessRestrictionRecord : DbaseRecord
    {
        private static readonly RoadSegmentAccessRestrictionSchema Schema = new RoadSegmentAccessRestrictionSchema();
        private static readonly RoadSegmentAccessRestrictionTranslator Translator = new RoadSegmentAccessRestrictionTranslator();

        public RoadSegmentAccessRestrictionRecord(RoadSegmentAccessRestriction value)
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