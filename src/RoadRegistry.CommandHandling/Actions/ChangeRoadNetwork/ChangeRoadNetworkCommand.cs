namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using RoadNetwork.Changes;

public class ChangeRoadNetworkCommand
{
    public Guid DownloadId { get; set; }
    public IList<ChangeRoadNetworkCommandItem> Changes { get; set; } = [];
    public Guid TicketId { get; set; }
}

// zelfde niveau als Messages namespace, komt dan in een SqsRequest terecht en wordt geserialized
public class ChangeRoadNetworkCommandItem
{
    // public AddGradeSeparatedJunctionCommand AddGradeSeparatedJunction { get; set; }
    // public AddRoadNodeCommand AddRoadNode { get; set; }
    public AddRoadSegmentChange AddRoadSegment { get; set; }
    // public AddRoadSegmentToEuropeanRoadCommand AddRoadSegmentToEuropeanRoad { get; set; }
    // public AddRoadSegmentToNationalRoadCommand AddRoadSegmentToNationalRoad { get; set; }
    // public AddRoadSegmentToNumberedRoadCommand AddRoadSegmentToNumberedRoad { get; set; }
    // public ModifyGradeSeparatedJunctionCommand ModifyGradeSeparatedJunction { get; set; }
    // public ModifyRoadNodeCommand ModifyRoadNode { get; set; }
    public ModifyRoadSegmentChange ModifyRoadSegment { get; set; }
    // public RemoveGradeSeparatedJunctionCommand RemoveGradeSeparatedJunction { get; set; }
    // public RemoveRoadNodeCommand RemoveRoadNode { get; set; }
    public RemoveRoadSegmentChange RemoveRoadSegment { get; set; }
    // public RemoveRoadSegmentsCommand RemoveRoadSegments { get; set; }
    // public RemoveOutlinedRoadSegmentCommand RemoveOutlinedRoadSegment { get; set; }
    // public RemoveOutlinedRoadSegmentFromRoadNetworkCommand RemoveOutlinedRoadSegmentFromRoadNetwork { get; set; }
    // public RemoveRoadSegmentFromEuropeanRoadCommand RemoveRoadSegmentFromEuropeanRoad { get; set; }
    // public RemoveRoadSegmentFromNationalRoadCommand RemoveRoadSegmentFromNationalRoad { get; set; }
    // public RemoveRoadSegmentFromNumberedRoadCommand RemoveRoadSegmentFromNumberedRoad { get; set; }
}
