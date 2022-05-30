namespace RoadRegistry.BackOffice
{
    using NodaTime;
    using NodaTime.Text;

    public static class EnrichEvent
    {
        public static EventEnricher WithTime(IClock clock)
        {
            var pattern = InstantPattern.ExtendedIso;
            return @event =>
            {
                switch (@event)
                {
                    // Import
                    case Messages.BeganRoadNetworkImport m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.CompletedRoadNetworkImport m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.ImportedOrganization m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.ImportedRoadNode m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.ImportedRoadSegment m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.ImportedGradeSeparatedJunction m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    // Uploads
                    case Messages.RoadNetworkChangesArchiveUploaded m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesArchiveAccepted m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesArchiveRejected m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    // Core
                    case Messages.NoRoadNetworkChanges m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesAccepted m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesRejected m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    // Extracts
                    case Messages.RoadNetworkExtractGotRequested m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkExtractGotRequestedV2 m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkExtractDownloadBecameAvailable m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkExtractChangesArchiveUploaded m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkExtractChangesArchiveAccepted m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkExtractChangesArchiveRejected m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                }
            };
        }
    }
}
