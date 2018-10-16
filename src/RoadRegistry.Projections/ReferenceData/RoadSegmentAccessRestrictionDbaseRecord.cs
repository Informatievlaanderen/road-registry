namespace RoadRegistry.Projections
{
    using Events;
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentAccessRestrictionDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentAccessRestrictionDbaseSchema Schema = new RoadSegmentAccessRestrictionDbaseSchema();
        private static readonly RoadSegmentAccessRestrictionTranslator Translator = new RoadSegmentAccessRestrictionTranslator();

        public RoadSegmentAccessRestrictionDbaseRecord(RoadSegmentAccessRestriction value)
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
