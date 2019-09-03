namespace RoadRegistry.BackOffice.Model
{
    using System.Collections.Generic;
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
    }
}
