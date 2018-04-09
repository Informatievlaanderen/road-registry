namespace RoadRegistry.Commands
{
    public enum SurfaceType
    {
        NotApplicable, // -9	niet van toepassing	Niet van toepassing
        Unknown, // -8	niet gekend	Geen informatie beschikbaar
        SolidHardening, // 1	weg met vaste verharding	Weg met een wegdek bestaande uit in vast verband aangebrachte materialen zoals asfalt, beton, klinkers, kasseien, enz.
        LooseHardening // 2	weg met losse verharding	Weg met een wegverharding bestaande uit losliggende materialen (bv. grind, kiezel, steenslag, puin, enz.).
    }
}