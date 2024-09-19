namespace RoadRegistry.Snapshot.Handlers.Sqs.Infrastructure;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;

public class SnapshotLambdaHealthCheckSqsRequest : SqsRequest
{
    public string? AssemblyVersion { get; init; }
}
