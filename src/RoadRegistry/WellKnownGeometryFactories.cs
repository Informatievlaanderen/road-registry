namespace RoadRegistry;

using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

public static class WellKnownGeometryFactories
{
    public static readonly GeometryFactory Default = GeometryConfiguration.GeometryFactory;
    public static readonly GeometryFactory WithoutSrid = new(GeometryConfiguration.GeometryFactory.PrecisionModel, 0, GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory);
    public static readonly GeometryFactory WithoutMAndZ = new(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID, new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY));
}
