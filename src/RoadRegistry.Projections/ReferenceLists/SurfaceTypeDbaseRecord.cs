namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;
    using Messages;

    public class SurfaceTypeDbaseRecord : DbaseRecord
    {
        private static readonly SurfaceTypeDbaseSchema Schema = new SurfaceTypeDbaseSchema();
        private static readonly SurfaceTypeTranslator Translator = new SurfaceTypeTranslator();

        public SurfaceTypeDbaseRecord(SurfaceType value)
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