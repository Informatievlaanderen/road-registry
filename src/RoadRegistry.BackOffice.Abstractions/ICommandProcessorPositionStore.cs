namespace RoadRegistry.BackOffice.Abstractions;

public interface ICommandProcessorPositionStore
{
    Task<int?> ReadVersion(string name, CancellationToken cancellationToken);

    Task WriteVersion(string name, int version, CancellationToken cancellationToken);
}