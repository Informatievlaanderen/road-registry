namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum RoadSegmentStatus
    {
        Unknown = -8, // -8	niet gekend	Geen informatie beschikbaar
        PermitRequested = 1, // 1	vergunning aangevraagd	Geometrie komt voor op officieel document in behandeling.
        BuildingPermitGranted = 2, // 2	bouwvergunning verleend	Geometrie komt voor op goedgekeurd, niet vervallen bouwdossier
        UnderConstruction = 3, // 3	in aanbouw	Aanvang der werken is gemeld.
        InUse = 4, // 4	in gebruik	Werken zijn opgeleverd.
        OutOfUse = 5 // 5	buiten gebruik	Fysieke weg is buiten gebruik gesteld maar niet gesloopt.
    }

    public class RoadSegmentStatusTranslator : EnumTranslator<RoadSegmentStatus>
    {
        protected override IDictionary<RoadSegmentStatus, string> DutchTranslations => _dutchTranslations;
        private static readonly IDictionary<RoadSegmentStatus, string> _dutchTranslations = new Dictionary<RoadSegmentStatus, string>
        {
            {RoadSegmentStatus.Unknown, "niet gekend" },
            {RoadSegmentStatus.PermitRequested, "vergunning aangevraagd" },
            {RoadSegmentStatus.BuildingPermitGranted, "bouwvergunning" },
            {RoadSegmentStatus.UnderConstruction, "in aanbouw" },
            {RoadSegmentStatus.InUse, "in gebruik" },
            {RoadSegmentStatus.OutOfUse, "buiten gebruik" },
        };
    }
}
