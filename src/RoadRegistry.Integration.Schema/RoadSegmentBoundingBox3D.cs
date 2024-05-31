namespace RoadRegistry.Integration.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentBoundingBox3D
{
    public double MaximumM { get; set; }
    public double MaximumX { get; set; }
    public double MaximumY { get; set; }
    public double MinimumM { get; set; }
    public double MinimumX { get; set; }
    public double MinimumY { get; set; }

    public BoundingBox3D ToBoundingBox3D()
    {
        return new BoundingBox3D(
            MinimumX,
            MinimumY,
            MaximumX,
            MaximumY,
            0.0,
            0.0,
            MinimumM,
            MaximumM
        );
    }
}