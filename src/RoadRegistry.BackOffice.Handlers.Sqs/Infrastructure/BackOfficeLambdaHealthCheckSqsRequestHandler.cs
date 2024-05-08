namespace RoadRegistry.BackOffice.Handlers.Sqs.Infrastructure;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using TicketingService.Abstractions;

public class BackOfficeLambdaHealthCheckSqsRequestHandler : SqsHandler<BackOfficeLambdaHealthCheckSqsRequest>
{
    public const string Action = "HealthCheck";

    public BackOfficeLambdaHealthCheckSqsRequestHandler(
        IBackOfficeS3SqsQueue sqsQueue,
        ITicketing ticketing,
        ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(BackOfficeLambdaHealthCheckSqsRequest request)
    {
        return Guid.NewGuid().ToString();
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, BackOfficeLambdaHealthCheckSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
