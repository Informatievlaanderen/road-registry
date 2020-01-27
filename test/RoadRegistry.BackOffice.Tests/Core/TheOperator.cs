namespace RoadRegistry.BackOffice.Core
{
    using Framework;
    using Messages;

    public static class TheOperator
    {
        public static Command ChangesTheRoadNetwork(params RequestedChange[] changes)
        {
            return new Command(new ChangeRoadNetwork
            {
                Changes = changes
            });
        }

        public static Command ChangesTheRoadNetworkBasedOnAnArchive(ArchiveId archive, Reason reason,
            OperatorName @operator, OrganizationId organization, params RequestedChange[] changes)
        {
            return new Command(new ChangeRoadNetworkBasedOnArchive
            {
                ArchiveId = archive,
                Reason = reason,
                Operator = @operator,
                OrganizationId = organization,
                Changes = changes
            });
        }
    }
}
