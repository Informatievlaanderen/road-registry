namespace RoadRegistry.Projections
{
    using Events;
    using Shaperon;

    public class RoadSegmentStatusRecord : DbaseRecord
    {
        private static readonly RoadSegmentStatusSchema Schema = new RoadSegmentStatusSchema();
        private static readonly RoadSegmentStatusTranslator Translator = new RoadSegmentStatusTranslator();

        public RoadSegmentStatusRecord(RoadSegmentStatus value)
        {
            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.STATUS, Translator.TranslateToIdentifier(value)), 
                new DbaseString(Schema.LBLSTATUS, Translator.TranslateToDutchName(value)), 
                new DbaseString(Schema.DEFSTATUS, Translator.TranslateToDutchDescription(value)), 
            };
        }
    }
}