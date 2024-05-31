namespace RoadRegistry.Integration.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeBoundingBox2D
{
    public double MaximumX { get; set; }
    public double MaximumY { get; set; }
    public double MinimumX { get; set; }
    public double MinimumY { get; set; }

    public BoundingBox3D ToBoundingBox3D(double minimumM = default, double maximumM = default)
    {
        return new BoundingBox3D(
            MinimumX,
            MinimumY,
            MaximumX,
            MaximumY,
            0.0,
            0.0,
            minimumM,
            maximumM
        );
    }
}