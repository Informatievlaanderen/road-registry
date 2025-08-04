namespace RoadRegistry.BackOffice.Extracts;

using Exceptions;

public class CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException : RoadRegistryException
{
    public CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException()
        : base("Can not upload a road network extract changes archive for the attempted download because it has been superseded by the required download.")
    {
    }
}
