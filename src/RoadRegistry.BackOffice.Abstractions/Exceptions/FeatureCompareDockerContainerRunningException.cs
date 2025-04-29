namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class FeatureCompareDockerContainerRunningException : Exception
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

    public ArchiveId ArchiveId { get; init; }
}
