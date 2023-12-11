namespace RoadRegistry.BackOffice.Core;

using System;

public static class DefaultTolerances
{
    public static readonly double GeometryTolerance = 1 / Math.Pow(10, Precisions.GeometryPrecision); // 0.001
}
