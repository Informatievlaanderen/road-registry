namespace RoadRegistry.Projections
{
    using Events;
    using Shaperon;

    public class RoadSegmentGeometryDrawMethodRecord : DbaseRecord
    {
        private static readonly RoadSegmentGeometryDrawMethodSchema Schema = new RoadSegmentGeometryDrawMethodSchema();
        private static readonly RoadSegmentGeometryDrawMethodTranslator Translator = new RoadSegmentGeometryDrawMethodTranslator();

        public RoadSegmentGeometryDrawMethodRecord(RoadSegmentGeometryDrawMethod value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.METHODE, Translator.TranslateToIdentifier(value)),
                new DbaseString(Schema.LBLMETHOD, Translator.TranslateToDutchName(value)),
                new DbaseString(Schema.DEFMETHOD, Translator.TranslateToDutchDescription(value)),
            };
        }
    }
}
