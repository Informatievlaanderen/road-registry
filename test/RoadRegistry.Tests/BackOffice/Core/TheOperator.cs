namespace RoadRegistry.BackOffice.Core
{
    using Framework;
    using Messages;

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
    }
}
