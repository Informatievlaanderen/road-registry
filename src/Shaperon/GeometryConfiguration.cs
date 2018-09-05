namespace Shaperon
{
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;

    public static class GeometryConfiguration
    {
        public static readonly IGeometryFactory GeometryFactory = new GeometryFactory(
                new PrecisionModel(PrecisionModels.Floating),
                SpatialReferenceSystemIdentifier.BelgeLambert1972,
                new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XYZM)
            );
    }
}
