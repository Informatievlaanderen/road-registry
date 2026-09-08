namespace RoadRegistry.BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using TicketingService.Abstractions;

public class ChangeGradeSeparatedJunctionToGradeJunctionSqsRequestHandler : SqsHandler<ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest>
{
    public const string Action = "ChangeGradeSeparatedJunctionToGradeJunction";

    public ChangeGradeSeparatedJunctionToGradeJunctionSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest request)
    {
        return Constants.GlobalRoadNetworkMessageGroupId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
