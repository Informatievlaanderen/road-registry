namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class FeatureCompareDockerContainerRunningException : ApplicationException
{
    public FeatureCompareDockerContainerRunningException(string message)
        : base(message)
    {
    }

    public FeatureCompareDockerContainerRunningException(string message, ArchiveId archiveId)
        : this(message)
    {
        ArchiveId = archiveId;
    }

    private FeatureCompareDockerContainerRunningException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public ArchiveId ArchiveId { get; init; }
}