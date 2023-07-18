namespace RoadRegistry.BackOffice.Core;

public static class DefaultTolerances
{
    public static readonly double DynamicRoadSegmentAttributePositionTolerance = 0.001;
    public static readonly double GeometryTolerance = 0.0015;
    public static readonly double MeasurementTolerance = 0.001;
    public static readonly double ClusterTolerance = 0.10; // cfr WVB in GRB = 0,15
    public static readonly double IntersectionBuffer = 1.0;
}
