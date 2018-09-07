namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using Commands;
    using Framework;

    public static class TheOperator
    {
        public static Message ChangesTheRoadNetwork(Commands.RoadNetworkChange[] changeset)
        {
            return new Message(new Dictionary<string, object>(), new ChangeRoadNetwork
            {
                Changeset = changeset
            });
        }
    }
}