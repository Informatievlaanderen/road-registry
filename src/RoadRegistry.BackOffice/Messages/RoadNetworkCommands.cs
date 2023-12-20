namespace RoadRegistry.BackOffice.Messages;

using System;

public static class RoadNetworkCommands
{
    public static readonly Type[] All =
    {
        typeof(AnnounceRoadNetworkExtractDownloadBecameAvailable),
        typeof(AnnounceRoadNetworkExtractDownloadTimeoutOccurred),
        typeof(ChangeRoadNetwork),
        typeof(RequestRoadNetworkExtract),
        typeof(RebuildRoadNetworkSnapshot),
        typeof(RebuildRoadNetworkSnapshotCompleted),
        typeof(CreateOrganization),
        typeof(CreateOrganizationRejected),
        typeof(DeleteOrganization),
        typeof(DeleteOrganizationRejected),
        typeof(RenameOrganization),
        typeof(RenameOrganizationRejected),
        typeof(ChangeOrganization),
        typeof(ChangeOrganizationRejected),
        typeof(UploadRoadNetworkChangesArchive),
        typeof(UploadRoadNetworkExtractChangesArchive)
    };
}
