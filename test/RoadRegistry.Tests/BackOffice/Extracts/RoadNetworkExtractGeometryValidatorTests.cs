namespace RoadRegistry.BackOffice.Extracts
{
    using FluentAssertions;
    using Messages;
    using NetTopologySuite.IO;
    using Xunit;

    public class RoadNetworkExtractGeometryValidatorTests
    {
        private readonly RoadNetworkExtractGeometryValidator _validator;
        private readonly WKTReader _reader;

        public RoadNetworkExtractGeometryValidatorTests()
        {
            _reader = new WKTReader();
            _validator = new RoadNetworkExtractGeometryValidator();
        }

        [Fact]
        public void ValidateCanHandleGeometryWithHoles()
        {
            var geometry = CreateGeometryWithHoles();

            var validationResult = _validator.Validate(geometry);

            validationResult.IsValid.Should().BeTrue();
        }

        private RoadNetworkExtractGeometry CreateGeometryWithHoles()
        {
            const int validSpatialReferenceSystemIdentifier = 1;
            var polygonal = _reader.Read(GeometryTranslatorTestCases.ValidGeometryWithHoles) as NetTopologySuite.Geometries.IPolygonal;

            var geometry = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(polygonal);
            geometry.SpatialReferenceSystemIdentifier = validSpatialReferenceSystemIdentifier;

            return geometry;
        }
    }
}
