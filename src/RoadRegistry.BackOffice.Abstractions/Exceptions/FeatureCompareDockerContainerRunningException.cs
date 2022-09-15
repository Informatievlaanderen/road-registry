namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class FeatureCompareDockerContainerRunningException : ApplicationException
{
    public FeatureCompareDockerContainerRunningException(string message) : base(message)
    {
    }

    public FeatureCompareDockerContainerRunningException(string message, ArchiveId archiveId) : this(message)
    {
        ArchiveId = archiveId;
    }

    public ArchiveId ArchiveId { get; init; }
}
