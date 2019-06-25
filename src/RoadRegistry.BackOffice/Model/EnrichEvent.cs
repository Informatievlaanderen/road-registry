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
                    case Messages.BeganRoadNetworkImport began:
                        began.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.CompletedRoadNetworkImport completed:
                        completed.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.ImportedOrganization organization:
                        organization.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.ImportedRoadNode roadNode:
                        roadNode.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.ImportedRoadSegment roadSegment:
                        roadSegment.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.ImportedGradeSeparatedJunction gradeSeparatedJunction:
                        gradeSeparatedJunction.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesArchiveUploaded uploaded:
                        uploaded.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesArchiveAccepted archiveAccepted:
                        archiveAccepted.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesArchiveRejected archiveRejected:
                        archiveRejected.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesAccepted changesAccepted:
                        changesAccepted.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                    case Messages.RoadNetworkChangesRejected changesRejected:
                        changesRejected.When = pattern.Format(clock.GetCurrentInstant());
                        break;
                }
            };
        }
    }
}
