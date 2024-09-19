namespace RoadRegistry.BackOffice.Handlers.Sqs.Infrastructure;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

[BlobRequest]
public class BackOfficeLambdaHealthCheckSqsRequest : SqsRequest
{
    public string? AssemblyVersion { get; init; }
}
