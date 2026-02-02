namespace RoadRegistry.ValueObjects;

using System;

public static class DefaultTolerances
{
    public static readonly double GeometryTolerance = 1 / Math.Pow(10, Precisions.GeometryPrecision); // 0.001
    public static readonly double GeometryToleranceV2 = 1 / Math.Pow(10, Precisions.GeometryPrecisionV2); // 0.01
    public static readonly double DuplicateCoordinatesTolerance = 0.0015;
}
