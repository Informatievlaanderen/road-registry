namespace RoadRegistry.Projections
{
    using Events;
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentGeometryDrawMethodDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentGeometryDrawMethodDbaseSchema Schema = new RoadSegmentGeometryDrawMethodDbaseSchema();
        private static readonly RoadSegmentGeometryDrawMethodTranslator Translator = new RoadSegmentGeometryDrawMethodTranslator();

        public RoadSegmentGeometryDrawMethodDbaseRecord(RoadSegmentGeometryDrawMethod value)
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
