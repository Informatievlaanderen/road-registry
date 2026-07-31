namespace RoadRegistry;

using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

public static class WellKnownGeometryFactories
{
    public static readonly GeometryFactory Lambert72 = GeometryConfiguration.GeometryFactory;
    public static readonly GeometryFactory Lambert72WithoutMAndZ = new(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID, new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY));

    // In road registry v2 geometries are plain 2D (no Z, no M), so use the shared 2D Lambert 2008 factory.
    public static readonly GeometryFactory Lambert08 = NtsGeometryFactory.CreateGeometryFactoryLambert2008();

    public static readonly GeometryFactory WithoutSrid = new(GeometryConfiguration.GeometryFactory.PrecisionModel, 0, GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory);
}
