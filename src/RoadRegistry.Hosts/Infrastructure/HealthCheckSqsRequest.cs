namespace RoadRegistry.Hosts.Infrastructure;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;

public class HealthCheckSqsRequest : SqsRequest
{
    public string MessageGroupId => "HealthCheck";
}
