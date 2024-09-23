namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.Runtime.Serialization;

[Serializable]
public class CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException : Exception
{
    public CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException()
        : base("Can not upload a road network extract changes archive for the same download more than once.")
    {
    }

    protected CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
    }
}
