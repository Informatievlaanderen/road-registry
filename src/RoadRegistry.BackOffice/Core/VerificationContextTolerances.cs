namespace RoadRegistry.BackOffice.Core;

using System;

public class VerificationContextTolerances
{
    public static readonly VerificationContextTolerances Default = new (
        DefaultTolerances.GeometryTolerance
    );

    public VerificationContextTolerances(
        double geometryTolerance
    )
    {
        if (geometryTolerance <= 0.0) throw new ArgumentOutOfRangeException(nameof(geometryTolerance));
        GeometryTolerance = geometryTolerance;
    }

    public double GeometryTolerance { get; }
}
