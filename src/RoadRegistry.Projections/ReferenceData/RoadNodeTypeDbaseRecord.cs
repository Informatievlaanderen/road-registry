namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;
    using Messages;

    public class RoadNodeTypeDbaseRecord : DbaseRecord
    {
        private static readonly RoadNodeTypeDbaseSchema Schema = new RoadNodeTypeDbaseSchema();
        private static readonly RoadNodeTypeTranslator Translator = new RoadNodeTypeTranslator();

        public RoadNodeTypeDbaseRecord(RoadNodeType value)
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