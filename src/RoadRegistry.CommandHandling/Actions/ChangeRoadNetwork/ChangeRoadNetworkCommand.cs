namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

public class ChangeRoadNetworkCommand
{
    public ICollection<ChangeRoadNetworkCommandItem> Changes { get; set; }
}

// zelfde niveau als Messages namespace, komt dan in een SqsRequest terecht en wordt geserialized
public class ChangeRoadNetworkCommandItem
{
    // public AddGradeSeparatedJunction AddGradeSeparatedJunction { get; set; }
    // public AddRoadNode AddRoadNode { get; set; }
    public AddRoadSegment AddRoadSegment { get; set; }
    // public AddRoadSegmentToEuropeanRoad AddRoadSegmentToEuropeanRoad { get; set; }
    // public AddRoadSegmentToNationalRoad AddRoadSegmentToNationalRoad { get; set; }
    // public AddRoadSegmentToNumberedRoad AddRoadSegmentToNumberedRoad { get; set; }
    // public ModifyGradeSeparatedJunction ModifyGradeSeparatedJunction { get; set; }
    // public ModifyRoadNode ModifyRoadNode { get; set; }
    // public ModifyRoadSegment ModifyRoadSegment { get; set; }
    // public RemoveGradeSeparatedJunction RemoveGradeSeparatedJunction { get; set; }
    // public RemoveRoadNode RemoveRoadNode { get; set; }
    // public RemoveRoadSegment RemoveRoadSegment { get; set; }
    // public RemoveRoadSegments RemoveRoadSegments { get; set; }
    // public RemoveOutlinedRoadSegment RemoveOutlinedRoadSegment { get; set; }
    // public RemoveOutlinedRoadSegmentFromRoadNetwork RemoveOutlinedRoadSegmentFromRoadNetwork { get; set; }
    // public RemoveRoadSegmentFromEuropeanRoad RemoveRoadSegmentFromEuropeanRoad { get; set; }
    // public RemoveRoadSegmentFromNationalRoad RemoveRoadSegmentFromNationalRoad { get; set; }
    // public RemoveRoadSegmentFromNumberedRoad RemoveRoadSegmentFromNumberedRoad { get; set; }
}

public class AddRoadSegment
{
    public string AccessRestriction { get; set; }
    // public string Category { get; set; }
    // public int EndNodeId { get; set; }
    // public RoadSegmentGeometry Geometry { get; set; }
    // public string GeometryDrawMethod { get; set; }
    // public RequestedRoadSegmentLaneAttribute[] Lanes { get; set; }
    // public int? LeftSideStreetNameId { get; set; }
    // public string MaintenanceAuthority { get; set; }
    // public string Morphology { get; set; }
    // public int? RightSideStreetNameId { get; set; }
    // public int StartNodeId { get; set; }
    // public string Status { get; set; }
    // public RequestedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    // public int TemporaryId { get; set; }
    // public int? OriginalId { get; set; }
    // public int? PermanentId { get; set; }
    // public RequestedRoadSegmentWidthAttribute[] Widths { get; set; }
}
