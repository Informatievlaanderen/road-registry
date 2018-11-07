﻿namespace RoadRegistry.Events
{
    using Aiv.Vbr.EventHandling;

    [EventName("RoadNetworkChangesetAccepted")]
    [EventDescription("Indicates the road network changes were accepted.")]
    public class RoadNetworkChangesAccepted
    {
        public AcceptedChange[] Changes { get; set; }
    }

}
