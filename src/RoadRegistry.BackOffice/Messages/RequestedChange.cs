namespace RoadRegistry.BackOffice.Messages;

public class RequestedChange
{
    public AddRoadNode AddRoadNode { get; set; }
    public ModifyRoadNode ModifyRoadNode { get; set; }
    public RemoveRoadNode RemoveRoadNode { get; set; }
    public AddRoadSegment AddRoadSegment { get; set; }
    public ModifyRoadSegment ModifyRoadSegment { get; set; }
    public RemoveRoadSegment RemoveRoadSegment { get; set; }
    public AddRoadSegmentToEuropeanRoad AddRoadSegmentToEuropeanRoad { get; set; }
    public RemoveRoadSegmentFromEuropeanRoad RemoveRoadSegmentFromEuropeanRoad { get; set; }
    public AddRoadSegmentToNationalRoad AddRoadSegmentToNationalRoad { get; set; }
    public RemoveRoadSegmentFromNationalRoad RemoveRoadSegmentFromNationalRoad { get; set; }
    public AddRoadSegmentToNumberedRoad AddRoadSegmentToNumberedRoad { get; set; }
    public ModifyRoadSegmentOnNumberedRoad ModifyRoadSegmentOnNumberedRoad { get; set; }
    public RemoveRoadSegmentFromNumberedRoad RemoveRoadSegmentFromNumberedRoad { get; set; }
    public AddGradeSeparatedJunction AddGradeSeparatedJunction { get; set; }
    public ModifyGradeSeparatedJunction ModifyGradeSeparatedJunction { get; set; }
    public RemoveGradeSeparatedJunction RemoveGradeSeparatedJunction { get; set; }
}
