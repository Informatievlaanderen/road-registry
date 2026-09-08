namespace RoadRegistry.BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using TicketingService.Abstractions;

public class ChangeGradeSeparatedJunctionAttributesV2SqsRequestHandler : SqsHandler<ChangeGradeSeparatedJunctionAttributesV2SqsRequest>
{
    public const string Action = "ChangeGradeSeparatedJunctionAttributes";

    public ChangeGradeSeparatedJunctionAttributesV2SqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeGradeSeparatedJunctionAttributesV2SqsRequest request)
    {
        return Constants.GlobalRoadNetworkMessageGroupId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeGradeSeparatedJunctionAttributesV2SqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
