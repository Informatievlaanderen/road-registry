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
        typeof(CreateOrganization),
        typeof(CreateOrganizationRejected),
        typeof(DeleteOrganization),
        typeof(DeleteOrganizationRejected),
        typeof(RenameOrganization),
        typeof(RenameOrganizationRejected),
        typeof(UploadRoadNetworkChangesArchive),
        typeof(UploadRoadNetworkExtractChangesArchive)
    };
}
