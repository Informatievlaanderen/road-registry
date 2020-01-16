namespace RoadRegistry.BackOffice.Model
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
                    case Messages.RoadNetworkChangesArchiveUploaded m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesArchiveAccepted m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesArchiveRejected m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesAccepted m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesRejected m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesBasedOnArchiveAccepted m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesBasedOnArchiveRejected m:
                        m.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                }
            };
        }
    }
}
