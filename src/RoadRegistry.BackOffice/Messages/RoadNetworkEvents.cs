namespace RoadRegistry.BackOffice.Messages;

using System;

public static class RoadNetworkEvents
{
    public static readonly Type[] All =
    {
        typeof(BeganRoadNetworkImport),
        typeof(CompletedRoadNetworkImport),
        typeof(ImportedGradeSeparatedJunction),
        typeof(ImportedRoadNode),
        typeof(ImportedRoadSegment),
        typeof(ImportedOrganization),
        typeof(CreateOrganizationAccepted),
        typeof(CreateOrganizationRejected),
        typeof(DeleteOrganizationAccepted),
        typeof(DeleteOrganizationRejected),
        typeof(RenameOrganizationAccepted),
        typeof(RenameOrganizationRejected),
        typeof(ChangeOrganizationAccepted),
        typeof(ChangeOrganizationRejected),
        typeof(RoadNetworkChangesArchiveAccepted),
        typeof(RoadNetworkChangesArchiveRejected),
        typeof(RoadNetworkChangesArchiveUploaded),
        typeof(RoadNetworkChangesArchiveFeatureCompareCompleted),
        typeof(NoRoadNetworkChanges),
        typeof(RoadNetworkChangesAccepted),
        typeof(RoadNetworkChangesRejected),
        typeof(RoadNetworkExtractGotRequested),
        typeof(RoadNetworkExtractGotRequestedV2),
        typeof(RoadNetworkExtractDownloaded),
        typeof(RoadNetworkExtractDownloadBecameAvailable),
        typeof(RoadNetworkExtractDownloadTimeoutOccurred),
        typeof(RoadNetworkExtractChangesArchiveAccepted),
        typeof(RoadNetworkExtractChangesArchiveRejected),
        typeof(RoadNetworkExtractChangesArchiveUploaded),
        typeof(RoadNetworkExtractChangesArchiveFeatureCompareCompleted),
        typeof(RoadNetworkExtractClosed),
        typeof(RebuildRoadNetworkSnapshotCompleted)
    };
}
