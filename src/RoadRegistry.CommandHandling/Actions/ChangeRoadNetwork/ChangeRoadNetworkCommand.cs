namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using GradeSeparatedJunction.Changes;
using RoadNode.Changes;
using RoadSegment.Changes;

public class ChangeRoadNetworkCommand
{
    public Guid DownloadId { get; set; }
    public Provenance Provenance { get; set; }
    public Guid TicketId { get; set; }
    public IList<ChangeRoadNetworkCommandItem> Changes { get; set; } = [];
}

public class ChangeRoadNetworkCommandItem
{
    public AddRoadNodeChange AddRoadNode { get; set; }
    public ModifyRoadNodeChange ModifyRoadNode { get; set; }
    public RemoveRoadNodeChange RemoveRoadNode { get; set; }

    public AddRoadSegmentChange AddRoadSegment { get; set; }
    public ModifyRoadSegmentChange ModifyRoadSegment { get; set; }
    public RemoveRoadSegmentChange RemoveRoadSegment { get; set; }
    public AddRoadSegmentToEuropeanRoadChange AddRoadSegmentToEuropeanRoad { get; set; }
    public AddRoadSegmentToNationalRoadChange AddRoadSegmentToNationalRoad { get; set; }
    public RemoveRoadSegmentFromEuropeanRoadChange RemoveRoadSegmentFromEuropeanRoad { get; set; }
    public RemoveRoadSegmentFromNationalRoadChange RemoveRoadSegmentFromNationalRoad { get; set; }

    public AddGradeSeparatedJunctionChange AddGradeSeparatedJunction { get; set; }
    public RemoveGradeSeparatedJunctionChange RemoveGradeSeparatedJunction { get; set; }

    // public RemoveRoadSegmentsChange RemoveRoadSegments { get; set; } //TODO-pr move to separate command
}
