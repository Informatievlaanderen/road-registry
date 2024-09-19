namespace RoadRegistry.BackOffice.Messages;

using System;

public static class RoadNetworkCommands
{
    public static readonly Type[] All =
    {
        typeof(CheckCommandHostHealth),
        typeof(AnnounceRoadNetworkExtractDownloadBecameAvailable),
        typeof(AnnounceRoadNetworkExtractDownloadTimeoutOccurred),
        typeof(ChangeRoadNetwork),
        typeof(RequestRoadNetworkExtract),
        typeof(CreateOrganization),
        typeof(DeleteOrganization),
        typeof(RenameOrganization),
        typeof(ChangeOrganization),
        typeof(UploadRoadNetworkChangesArchive),
        typeof(UploadRoadNetworkExtractChangesArchive)
    };
}
