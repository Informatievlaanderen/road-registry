namespace RoadRegistry.BackOffice;

using Messages;
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
                case BeganRoadNetworkImport m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case CompletedRoadNetworkImport m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case ImportedOrganization m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case ImportedRoadNode m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case ImportedRoadSegment m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case ImportedGradeSeparatedJunction m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                // Uploads
                case RoadNetworkChangesArchiveUploaded m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkChangesArchiveFeatureCompareCompleted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkChangesArchiveAccepted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkChangesArchiveRejected m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                // Core
                case NoRoadNetworkChanges m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkChangesAccepted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkChangesRejected m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                // Organization
                case CreateOrganizationAccepted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case CreateOrganizationRejected m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case DeleteOrganizationAccepted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case DeleteOrganizationRejected m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RenameOrganizationAccepted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RenameOrganizationRejected m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case ChangeOrganizationAccepted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case ChangeOrganizationRejected m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                // Extracts
                case RoadNetworkExtractGotRequested m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkExtractGotRequestedV2 m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkExtractDownloaded m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkExtractClosed m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkExtractDownloadBecameAvailable m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkExtractDownloadTimeoutOccurred m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkExtractChangesArchiveUploaded m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkExtractChangesArchiveAccepted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkExtractChangesArchiveRejected m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RoadNetworkExtractChangesArchiveFeatureCompareCompleted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case RebuildRoadNetworkSnapshotCompleted m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                // Street Name
                case StreetNameCreated m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case StreetNameModified m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
                case StreetNameRemoved m:
                    m.When = pattern.Format(clock.GetCurrentInstant());
                    break;
            }
        };
    }
}
