namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;
    using Messages;

    public class RoadSegmentStatusDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentStatusDbaseSchema Schema = new RoadSegmentStatusDbaseSchema();
        private static readonly RoadSegmentStatusTranslator Translator = new RoadSegmentStatusTranslator();

        public RoadSegmentStatusDbaseRecord(RoadSegmentStatus value)
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
