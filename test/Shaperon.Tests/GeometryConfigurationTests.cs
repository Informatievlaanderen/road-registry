namespace Shaperon
{
    using GeoAPI.Geometries;
    using Xunit;

    public class GeometryConfigurationTests
    {
        [Fact]
        public void ConfiguredPrecionModelTypeIsFloating()
        {
            Assert.Equal(PrecisionModels.Floating, GeometryConfiguration.PrecisionModel.PrecisionModelType);
        }

        [Fact]
        public void ConfiguredCoordinateSequenceFactoryUsesOrdinatesXYZM()
        {
            Assert.Equal(Ordinates.XYZM, GeometryConfiguration.CoordinateSequenceFactory.Ordinates);
        }

        [Fact]
        public void ConfiguredGeometryFactoryUsesBelgeLambert1972SpatialRefernce()
        {
            Assert.Equal(SpatialReferenceSystemIdentifier.BelgeLambert1972, GeometryConfiguration.GeometryFactory.SRID);
        }

        [Fact]
        public void ConfiguredGeometryFactoryUsesTheConfiguredPrecisionModel()
        {
            Assert.Same(GeometryConfiguration.PrecisionModel, GeometryConfiguration.GeometryFactory.PrecisionModel);
        }

        [Fact]
        public void ConfiguredGeometryFactoryUsesTheConfiguredCoordinateSequenceFactory()
        {
            Assert.Equal(GeometryConfiguration.CoordinateSequenceFactory, GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory);
        }
    }
}
