namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using Aiv.Vbr.Shaperon;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class RoadSegmentGeometryValidatorTests
    {
        public RoadSegmentGeometryValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizePolylineM();

            Validator = new RoadSegmentGeometryValidator();
        }

        public Fixture Fixture { get; }

        public RoadSegmentGeometryValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void SpatialReferenceSystemIdentifierMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.SpatialReferenceSystemIdentifier, value);
        }

        [Fact]
        public void MultiLineStringCanNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.MultiLineString, (Messages.LineString[])null);
        }

        [Fact]
        public void LineStringCanNotBeNull()
        {
            var data = Fixture.CreateMany<Messages.LineString>(10).ToArray();
            var index = new Random().Next(0, data.Length);
            data[index] = null;

            Validator.ShouldHaveValidationErrorFor(c => c.MultiLineString, data);
        }

        [Fact]
        public void LineStringHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.MultiLineString, typeof(LineStringValidator));
        }

        [Fact]
        public void VerifyValid()
        {
            Fixture.CustomizePolylineM();

            var data = GeometryTranslator.Translate(Fixture.Create<MultiLineString>());
            data.SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972;

            Validator.ValidateAndThrow(data);
        }
    }
}
