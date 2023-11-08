namespace RoadRegistry.BackOffice.Core;

using System;

public static class DefaultTolerances
{
    public static readonly double GeometryTolerance = 0.0015;
    public static readonly double MeasurementTolerance = 1 / Math.Pow(10, Precisions.MeasurementPrecision); // 0.001
    public static readonly double ClusterTolerance = 0.10; // cfr WVB in GRB = 0,15
    public static readonly double IntersectionBuffer = 1.0;
}
