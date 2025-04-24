namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class FeatureCompareDockerContainerNotRunningException : Exception
{
    public FeatureCompareDockerContainerNotRunningException(string message, ArchiveId archiveId)
        : base(message)
    {
        ArchiveId = archiveId;
    }

    public ArchiveId ArchiveId { get; init; }
}
