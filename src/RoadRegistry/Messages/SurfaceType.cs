namespace RoadRegistry.Messages
{
    using System;
    using System.Collections.Generic;

    [Obsolete]
    public enum SurfaceType
    {
        NotApplicable = -9,
        Unknown = -8,
        SolidSurface = 1,
        LooseSurface = 2
    }

    [Obsolete("Use SurfaceType.Translation instead")]
    public class SurfaceTypeTranslator : EnumTranslator<SurfaceType>
    {
        protected override IDictionary<SurfaceType, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<SurfaceType, string> DutchDescriptions => _dutchDescriptions;

        private static readonly IDictionary<SurfaceType, string> _dutchTranslations =
            new Dictionary<SurfaceType, string>
            {
                { SurfaceType.NotApplicable, "niet van toepassing" },
                { SurfaceType.Unknown, "niet gekend" },
                { SurfaceType.SolidSurface, "weg met vaste verharding" },
                { SurfaceType.LooseSurface, "weg met losse verharding" },
            };

        private static readonly IDictionary<SurfaceType, string> _dutchDescriptions =
            new Dictionary<SurfaceType, string>
            {
                { SurfaceType.NotApplicable, "Niet van toepassing" },
                { SurfaceType.Unknown, "Geen informatie beschikbaar" },
                { SurfaceType.SolidSurface, "Weg met een wegdek bestaande uit in vast verband aangebrachte materialen zoals asfalt, beton, klinkers, kasseien, enz." },
                { SurfaceType.LooseSurface, "Weg met een wegverharding bestaande uit losliggende materialen (bv. grind, kiezel, steenslag, puin, enz.)." },
            };
    }
}
