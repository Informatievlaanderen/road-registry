namespace RoadRegistry.Projections
{
    using Events;
    using Aiv.Vbr.Shaperon;

    public class ReferencePointTypeDbaseRecord : DbaseRecord
    {
        public static readonly ReferencePointTypeDbaseSchema Schema = new ReferencePointTypeDbaseSchema();
        private static readonly ReferencePointTypeTranslator Translator = new ReferencePointTypeTranslator();

        public ReferencePointTypeDbaseRecord(ReferencePointType type)
        {

            Values = new DbaseFieldValue[]
            {
                new DbaseInt32(Schema.TYPE, Translator.TranslateToIdentifier(type)),
                new DbaseString(Schema.LBLTYPE, Translator.TranslateToDutchName(type)),
                new DbaseString(Schema.DEFTYPE, Translator.TranslateToDutchDescription(type)),
            };
        }
    }
}
