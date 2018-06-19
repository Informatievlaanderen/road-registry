namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum HardeningType
    {
        NotApplicable = -9, // -9	niet van toepassing	Niet van toepassing
        Unknown = -8, // -8	niet gekend	Geen informatie beschikbaar
        SolidHardening = 1, // 1	weg met vaste verharding	Weg met een wegdek bestaande uit in vast verband aangebrachte materialen zoals asfalt, beton, klinkers, kasseien, enz.
        LooseHardening = 2 // 2	weg met losse verharding	Weg met een wegverharding bestaande uit losliggende materialen (bv. grind, kiezel, steenslag, puin, enz.).
    }

    public class HardeningTypeTranslator : EnumTranslator<HardeningType>
    {
        protected override IDictionary<HardeningType, string> DutchTranslations => _dutchTranslations;
        private static readonly IDictionary<HardeningType, string> _dutchTranslations =
            new Dictionary<HardeningType, string>
            {
                { HardeningType.NotApplicable, "niet van toepassing" },
                { HardeningType.Unknown, "niet gekend" },
                { HardeningType.SolidHardening, "weg met vaste verharding" },
                { HardeningType.LooseHardening, "weg met losse verharding" },
            };
    }
}
