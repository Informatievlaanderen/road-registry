namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class FeatureCompareDockerContainerNotRunningException : ApplicationException
{
    public FeatureCompareDockerContainerNotRunningException(string message, ArchiveId archiveId) : base(message)
    {
        ArchiveId = archiveId;
    }

    public ArchiveId ArchiveId { get; init; }
}
