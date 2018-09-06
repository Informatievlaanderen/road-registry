namespace RoadRegistry.Projections
{
    using Events;
    using Aiv.Vbr.Shaperon;
    using Shared;

    public class HardeningTypeDbaseRecord : DbaseRecord
    {
        private static readonly HardeningTypeDbaseSchema Schema = new HardeningTypeDbaseSchema();
        private static readonly HardeningTypeTranslator Translator = new HardeningTypeTranslator();

        public HardeningTypeDbaseRecord(HardeningType value)
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