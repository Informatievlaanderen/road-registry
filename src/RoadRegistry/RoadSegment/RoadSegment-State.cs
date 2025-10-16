namespace RoadRegistry.RoadSegment;

using BackOffice;
using NetTopologySuite.Geometries;

public partial class RoadSegment
{
    public RoadSegmentId Id { get; }
    public Geometry Geometry { get; }

    // When<...> methods
}
