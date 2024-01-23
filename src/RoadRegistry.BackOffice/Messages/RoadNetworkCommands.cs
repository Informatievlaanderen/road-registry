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
        typeof(CreateOrganization),
        typeof(DeleteOrganization),
        typeof(RenameOrganization),
        typeof(ChangeOrganization),
        typeof(UnlinkRoadSegmentsFromStreetName),
        typeof(UploadRoadNetworkChangesArchive),
        typeof(UploadRoadNetworkExtractChangesArchive)
    };
}
