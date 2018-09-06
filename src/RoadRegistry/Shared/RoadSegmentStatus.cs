namespace RoadRegistry.Shared
{
    using System.Collections.Generic;

    public enum RoadSegmentStatus
    {
        Unknown = -8,
        PermitRequested = 1,
        BuildingPermitGranted = 2,
        UnderConstruction = 3,
        InUse = 4,
        OutOfUse = 5
    }

    public class RoadSegmentStatusTranslator : EnumTranslator<RoadSegmentStatus>
    {
        protected override IDictionary<RoadSegmentStatus, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<RoadSegmentStatus, string> DutchDescriptions => _dutchDescriptions;

        private static readonly IDictionary<RoadSegmentStatus, string> _dutchTranslations =
            new Dictionary<RoadSegmentStatus, string>
            {
                { RoadSegmentStatus.Unknown, "niet gekend" },
                { RoadSegmentStatus.PermitRequested, "vergunning aangevraagd" },
                { RoadSegmentStatus.BuildingPermitGranted, "bouwvergunning verleend" },
                { RoadSegmentStatus.UnderConstruction, "in aanbouw" },
                { RoadSegmentStatus.InUse, "in gebruik" },
                { RoadSegmentStatus.OutOfUse, "buiten gebruik" },
            };

        private static readonly IDictionary<RoadSegmentStatus, string> _dutchDescriptions =
            new Dictionary<RoadSegmentStatus, string>
            {
                { RoadSegmentStatus.Unknown, "Geen informatie beschikbaar" },
                { RoadSegmentStatus.PermitRequested, "Geometrie komt voor op officieel document in behandeling." },
                { RoadSegmentStatus.BuildingPermitGranted, "Geometrie komt voor op goedgekeurd, niet vervallen bouwdossier." },
                { RoadSegmentStatus.UnderConstruction, "Aanvang der werken is gemeld." },
                { RoadSegmentStatus.InUse, "Werken zijn opgeleverd." },
                { RoadSegmentStatus.OutOfUse, "Fysieke weg is buiten gebruik gesteld maar niet gesloopt." },
            };
    }
}
