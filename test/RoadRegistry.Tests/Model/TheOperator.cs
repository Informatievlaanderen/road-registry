namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using Framework;
    using Messages;

    public static class TheOperator
    {
        public static Message ChangesTheRoadNetwork(params RequestedChange[] changes)
        {
            return new Message(new Dictionary<string, object>(), new ChangeRoadNetwork
            {
                Changes = changes
            });
        }
    }
}