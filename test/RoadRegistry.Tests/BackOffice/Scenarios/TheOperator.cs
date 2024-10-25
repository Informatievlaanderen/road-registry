namespace RoadRegistry.Tests.BackOffice.Scenarios;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;

public static class TheOperator
{
    public static Command ChangesTheRoadNetwork(
        ChangeRequestId requestId,
        Reason reason,
        OperatorName @operator,
        OrganizationId organization,
        params RequestedChange[] changes)
    {
        return new Command(new ChangeRoadNetwork
        {
            RequestId = requestId,
            Reason = reason,
            Operator = @operator,
            OrganizationId = organization,
            Changes = changes
        });
    }

    public static Command ChangesTheRoadNetwork(
        ChangeRequestId requestId,
        Reason reason,
        OperatorName @operator,
        OrganizationId organization,
        TicketId ticketId,
        params RequestedChange[] changes)
    {
        return new Command(new ChangeRoadNetwork
        {
            RequestId = requestId,
            Reason = reason,
            Operator = @operator,
            OrganizationId = organization,
            Changes = changes,
            TicketId = ticketId
        });
    }

    public static Command ChangesTheRoadNetwork(
        ChangeRequestId requestId,
        Reason reason,
        OperatorName @operator,
        OrganizationId organization,
        ExtractRequestId extractRequestId,
        params RequestedChange[] changes)
    {
        return new Command(new ChangeRoadNetwork
        {
            RequestId = requestId,
            Reason = reason,
            Operator = @operator,
            OrganizationId = organization,
            Changes = changes,
            ExtractRequestId = extractRequestId
        });
    }
}
