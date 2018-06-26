namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum HardeningType
    {
        NotApplicable = -9,
        Unknown = -8,
        SolidHardening = 1,
        LooseHardening = 2
    }

    public class HardeningTypeTranslator : EnumTranslator<HardeningType>
    {
        protected override IDictionary<HardeningType, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<HardeningType, string> DutchDescriptions => _dutchDescriptions;

        private static readonly IDictionary<HardeningType, string> _dutchTranslations =
            new Dictionary<HardeningType, string>
            {
                { HardeningType.NotApplicable, "niet van toepassing" },
                { HardeningType.Unknown, "niet gekend" },
                { HardeningType.SolidHardening, "weg met vaste verharding" },
                { HardeningType.LooseHardening, "weg met losse verharding" },
            };

        private static readonly IDictionary<HardeningType, string> _dutchDescriptions =
            new Dictionary<HardeningType, string>
            {
                { HardeningType.NotApplicable, "Niet van toepassing" },
                { HardeningType.Unknown, "Geen informatie beschikbaar" },
                { HardeningType.SolidHardening, "Weg met een wegdek bestaande uit in vast verband aangebrachte materialen zoals asfalt, beton, klinkers, kasseien, enz." },
                { HardeningType.LooseHardening, "Weg met een wegverharding bestaande uit losliggende materialen (bv. grind, kiezel, steenslag, puin, enz.)." },
            };
    }
}
