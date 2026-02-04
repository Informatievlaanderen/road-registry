namespace RoadRegistry;

using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

public static class WellknownSrids
{
    public const int Lambert72 = 31370;
    public const int Lambert08 = 3812;
}

public static class WellKnownGeometryFactories
{
    public static readonly GeometryFactory Lambert72 = GeometryConfiguration.GeometryFactory;
    public static readonly GeometryFactory Lambert72WithoutMAndZ = new(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID, new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY));

    public static readonly GeometryFactory Lambert08 = new(
        new PrecisionModel(PrecisionModels.Floating),
        WellknownSrids.Lambert08,
        new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XYZM));


    public static readonly GeometryFactory WithoutSrid = new(GeometryConfiguration.GeometryFactory.PrecisionModel, 0, GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory);
}
