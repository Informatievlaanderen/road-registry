namespace RoadRegistry.ValueObjects;

using System;

public class VerificationContextTolerances
{
    public static readonly VerificationContextTolerances Default = new (DefaultTolerances.GeometryTolerance);
    public static readonly VerificationContextTolerances Cm = new (0.01);
    public static readonly VerificationContextTolerances RoadNodeBuffer = new (0.05);
    public static readonly VerificationContextTolerances WithinRoadNodeBuffer = new (0.0499999);

    private VerificationContextTolerances(
        double geometryTolerance
    )
    {
        if (geometryTolerance <= 0.0) throw new ArgumentOutOfRangeException(nameof(geometryTolerance));
        GeometryTolerance = geometryTolerance;
    }

    public double GeometryTolerance { get; }
}
