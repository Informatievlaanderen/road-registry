namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class FeatureCompareDockerContainerNotRunningException : ApplicationException
{
    public FeatureCompareDockerContainerNotRunningException(string message, ArchiveId archiveId)
        : base(message)
    {
        ArchiveId = archiveId;
    }
    
    private FeatureCompareDockerContainerNotRunningException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    public ArchiveId ArchiveId { get; init; }
}
