namespace RoadRegistry.Snapshot.Handlers.Sqs.Infrastructure;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class SnapshotLambdaHealthCheckSqsRequestHandler : SqsHandler<SnapshotLambdaHealthCheckSqsRequest>
{
    public const string Action = "HealthCheck";

    public SnapshotLambdaHealthCheckSqsRequestHandler(
        SnapshotSqsQueue sqsQueue,
        ITicketing ticketing,
        ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(SnapshotLambdaHealthCheckSqsRequest request)
    {
        return Guid.NewGuid().ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SnapshotLambdaHealthCheckSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
