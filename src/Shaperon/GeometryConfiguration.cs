namespace Shaperon
{
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;

    public static class GeometryConfiguration
    {
        public static readonly IPrecisionModel PrecisionModel = new PrecisionModel(PrecisionModels.Floating);
        public static readonly ICoordinateSequenceFactory CoordinateSequenceFactory = new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XYZM);
        public static readonly IGeometryFactory GeometryFactory = new GeometryFactory(
                PrecisionModel,
                SpatialReferenceSystemIdentifier.BelgeLambert1972,
                CoordinateSequenceFactory
            );
    }
}
