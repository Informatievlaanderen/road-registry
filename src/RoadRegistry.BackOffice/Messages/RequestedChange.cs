namespace RoadRegistry.BackOffice.Messages;

public class RequestedChange
{
    public AddGradeSeparatedJunction AddGradeSeparatedJunction { get; set; }
    public AddRoadNode AddRoadNode { get; set; }
    public AddRoadSegment AddRoadSegment { get; set; }
    public AddRoadSegmentToEuropeanRoad AddRoadSegmentToEuropeanRoad { get; set; }
    public AddRoadSegmentToNationalRoad AddRoadSegmentToNationalRoad { get; set; }
    public AddRoadSegmentToNumberedRoad AddRoadSegmentToNumberedRoad { get; set; }
    public ModifyGradeSeparatedJunction ModifyGradeSeparatedJunction { get; set; }
    public ModifyRoadNode ModifyRoadNode { get; set; }
    public ModifyRoadSegment ModifyRoadSegment { get; set; }
    public ModifyRoadSegmentAttributes ModifyRoadSegmentAttributes { get; set; }
    public ModifyRoadSegmentGeometry ModifyRoadSegmentGeometry { get; set; }
    public ModifyRoadSegmentOnNumberedRoad ModifyRoadSegmentOnNumberedRoad { get; set; } //TODO-rik remove? Is not in use and no event exists with it
    public RemoveGradeSeparatedJunction RemoveGradeSeparatedJunction { get; set; }
    public RemoveRoadNode RemoveRoadNode { get; set; }
    public RemoveRoadSegment RemoveRoadSegment { get; set; }
    public RemoveOutlinedRoadSegment RemoveOutlinedRoadSegment { get; set; }
    public RemoveOutlinedRoadSegmentFromRoadNetwork RemoveOutlinedRoadSegmentFromRoadNetwork { get; set; }
    public RemoveRoadSegmentFromEuropeanRoad RemoveRoadSegmentFromEuropeanRoad { get; set; }
    public RemoveRoadSegmentFromNationalRoad RemoveRoadSegmentFromNationalRoad { get; set; }
    public RemoveRoadSegmentFromNumberedRoad RemoveRoadSegmentFromNumberedRoad { get; set; }
}
