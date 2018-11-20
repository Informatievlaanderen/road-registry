namespace RoadRegistry.Model
{
    using Aiv.Vbr.Shaperon;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class RoadNodeGeometryValidatorTests
    {
        public RoadNodeGeometryValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizePolylineM();

            Validator = new RoadNodeGeometryValidator();
        }

        public Fixture Fixture { get; }

        public RoadNodeGeometryValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void SpatialReferenceSystemIdentifierMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.SpatialReferenceSystemIdentifier, value);
        }

        [Fact]
        public void PointCanNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Point, (Messages.Point)null);
        }

        [Fact]
        public void VerifyValid()
        {
            Fixture.CustomizePointM();

            var data = GeometryTranslator.Translate(Fixture.Create<PointM>());
            data.SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972;

            Validator.ValidateAndThrow(data);
        }
    }
}