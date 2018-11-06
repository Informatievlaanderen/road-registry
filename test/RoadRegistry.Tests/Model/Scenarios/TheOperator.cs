namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using Commands;
    using Framework;

    public static class TheOperator
    {
        public static Message ChangesTheRoadNetwork(params Commands.RoadNetworkChange[] changes)
        {
            return new Message(new Dictionary<string, object>(), new ChangeRoadNetwork
            {
                Changes = changes
            });
        }
    }
}