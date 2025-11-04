namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadNetwork.Changes;

public class ChangeRoadNetworkCommand
{
    public Guid DownloadId { get; set; }
    public Provenance Provenance { get; set; }
    public Guid TicketId { get; set; }
    public IList<ChangeRoadNetworkCommandItem> Changes { get; set; } = [];
}

// zelfde niveau als Messages namespace, komt dan in een SqsRequest terecht en wordt geserialized
public class ChangeRoadNetworkCommandItem
{
    public AddGradeSeparatedJunctionChange AddGradeSeparatedJunction { get; set; }
    public AddRoadNodeChange AddRoadNode { get; set; }
    public AddRoadSegmentChange AddRoadSegment { get; set; }
    // public AddRoadSegmentToEuropeanRoadChange AddRoadSegmentToEuropeanRoad { get; set; }
    // public AddRoadSegmentToNationalRoadChange AddRoadSegmentToNationalRoad { get; set; }
    // public ModifyGradeSeparatedJunctionChange ModifyGradeSeparatedJunction { get; set; }
    // public ModifyRoadNodeChange ModifyRoadNode { get; set; }
    public ModifyRoadSegmentChange ModifyRoadSegment { get; set; }
    // public RemoveGradeSeparatedJunctionChange RemoveGradeSeparatedJunction { get; set; }
    // public RemoveRoadNodeChange RemoveRoadNode { get; set; }
    public RemoveRoadSegmentChange RemoveRoadSegment { get; set; }
    // public RemoveRoadSegmentFromEuropeanRoadChange RemoveRoadSegmentFromEuropeanRoad { get; set; }
    // public RemoveRoadSegmentFromNationalRoadChange RemoveRoadSegmentFromNationalRoad { get; set; }

    // public RemoveRoadSegmentsChange RemoveRoadSegments { get; set; } //TODO-pr move to separate command
}
