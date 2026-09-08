namespace RoadRegistry.BackOffice.Handlers.Sqs.GradeJunctions.V2;

using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using TicketingService.Abstractions;

public class ChangeGradeJunctionToGradeSeparatedJunctionSqsRequestHandler : SqsHandler<ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest>
{
    public const string Action = "ChangeGradeJunctionToGradeSeparatedJunction";

    public ChangeGradeJunctionToGradeSeparatedJunctionSqsRequestHandler(IBackOfficeS3SqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
    {
    }

    protected override string WithAggregateId(ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest request)
    {
        return Constants.GlobalRoadNetworkMessageGroupId;
    }

    protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest sqsRequest)
    {
        return new Dictionary<string, string>
        {
            { RegistryKey, nameof(RoadRegistry) },
            { ActionKey, Action },
            { AggregateIdKey, aggregateId }
        };
    }
}
