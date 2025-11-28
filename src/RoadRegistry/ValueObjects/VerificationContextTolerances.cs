namespace RoadRegistry.ValueObjects;

using System;

public class VerificationContextTolerances
{
    public static readonly VerificationContextTolerances Default = new (
        DefaultTolerances.GeometryTolerance
    );

    private VerificationContextTolerances(
        double geometryTolerance
    )
    {
        if (geometryTolerance <= 0.0) throw new ArgumentOutOfRangeException(nameof(geometryTolerance));
        GeometryTolerance = geometryTolerance;
    }

    public double GeometryTolerance { get; }
}
