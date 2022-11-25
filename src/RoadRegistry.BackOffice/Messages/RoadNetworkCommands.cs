namespace RoadRegistry.BackOffice.Messages;

using System;

public static class RoadNetworkCommands
{
    public static readonly Type[] All =
    {
        typeof(AnnounceRoadNetworkExtractDownloadBecameAvailable),
        typeof(ChangeRoadNetwork),
        typeof(RequestRoadNetworkExtract),
        typeof(RebuildRoadNetworkSnapshot),
        typeof(RenameOrganization),
        typeof(RenameOrganizationRejected),
        typeof(UploadRoadNetworkChangesArchive),
        typeof(UploadRoadNetworkExtractChangesArchive)
    };
}
