namespace RoadRegistry.Projections
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Aiv.Vbr.Shaperon;
    using Messages;

    public class GradeSeparatedJunctionTypeDbaseRecord : DbaseRecord
    {
        private static readonly GradeSeparatedJunctionTypeDbaseSchema Schema = new GradeSeparatedJunctionTypeDbaseSchema();
        private static readonly GradeSeparatedJunctionTypeTranslator Translator = new GradeSeparatedJunctionTypeTranslator();

        public static readonly GradeSeparatedJunctionTypeDbaseRecord[] All =
            Array.ConvertAll(
                ((GradeSeparatedJunctionType[])Enum.GetValues(typeof(GradeSeparatedJunctionType)))
                .OrderBy(value => ((IConvertible)value).ToInt32(CultureInfo.InvariantCulture))
                .ToArray(),
                candidate => new GradeSeparatedJunctionTypeDbaseRecord(candidate)
            );

        public GradeSeparatedJunctionTypeDbaseRecord(GradeSeparatedJunctionType value)
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
