namespace RoadRegistry.BackOffice.Extracts;

using System;

public class CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException : Exception
{
    public CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException()
        : base("Can not upload a road network extract changes archive for the same download more than once.")
    {
    }
}
