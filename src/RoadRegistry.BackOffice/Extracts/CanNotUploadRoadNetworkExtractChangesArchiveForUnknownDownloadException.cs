namespace RoadRegistry.BackOffice.Extracts;

using Exceptions;

public class CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException : RoadRegistryException
{
    public CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException()
        : base("Can not upload a road network extract archive for the unknown download.")
    {
    }
}
