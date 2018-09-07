namespace Shaperon
{
    using GeoAPI.Geometries;
    using Xunit;

    public class GeometryConfigurationTests
    {
        private readonly IGeometryFactory _configuredFactory;

        public GeometryConfigurationTests()
        {
            _configuredFactory = GeometryConfiguration.GeometryFactory;
        }

        [Fact]
        public void ConfiguredGeometryFactoryHasFloatingPrecision()
        {
            var factoryPrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel;
            Assert.Equal(PrecisionModels.Floating, factoryPrecisionModel.PrecisionModelType);
        }

        [Fact]
        public void ConfiguredGeometryFactoryHasOrdinatesXYZM()
        {
            var coordinateSequenceFactory = GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory;
            Assert.Equal(Ordinates.XYZM, coordinateSequenceFactory.Ordinates);
        }

        [Fact]
        public void ConfiguredGeometryFactoryUsesBelgeLambert1972SpatialRefernce()
        {
            Assert.Equal(SpatialReferenceSystemIdentifier.BelgeLambert1972, GeometryConfiguration.GeometryFactory.SRID);
        }
    }
}
