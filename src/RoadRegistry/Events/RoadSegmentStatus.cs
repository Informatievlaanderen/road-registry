namespace RoadRegistry.Events
{
    public enum RoadSegmentStatus
    {
        Unknown, // -8	niet gekend	Geen informatie beschikbaar
        PermitRequested, // 1	vergunning aangevraagd	Geometrie komt voor op officieel document in behandeling.
        BuildingPermitGranted, // 2	bouwvergunning verleend	Geometrie komt voor op goedgekeurd, niet vervallen bouwdossier
        UnderConstruction, // 3	in aanbouw	Aanvang der werken is gemeld.
        InUse, // 4	in gebruik	Werken zijn opgeleverd.
        OutOfUse // 5	buiten gebruik	Fysieke weg is buiten gebruik gesteld maar niet gesloopt.
    }
}