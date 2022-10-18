namespace RoadRegistry.Dbase;

public class RoadNetworkInfo
{
    public bool CompletedImport { get; set; }
    public int GradeSeparatedJunctionCount { get; set; }

    public int Id { get; set; } = Identifier;
    public const int Identifier = 0;
    public int OrganizationCount { get; set; }
    public int RoadNodeCount { get; set; }
    public int RoadSegmentCount { get; set; }
    public int RoadSegmentEuropeanRoadAttributeCount { get; set; }
    public int RoadSegmentLaneAttributeCount { get; set; }
    public int RoadSegmentNationalRoadAttributeCount { get; set; }
    public int RoadSegmentNumberedRoadAttributeCount { get; set; }
    public int RoadSegmentSurfaceAttributeCount { get; set; }
    public int RoadSegmentWidthAttributeCount { get; set; }
    public int TotalRoadNodeShapeLength { get; set; }
    public int TotalRoadSegmentShapeLength { get; set; }
}
