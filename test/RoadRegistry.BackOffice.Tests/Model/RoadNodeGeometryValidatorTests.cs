namespace RoadRegistry.BackOffice.Model
{
    using Aiv.Vbr.Shaperon;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Messages;
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
            Validator.ShouldHaveValidationErrorFor(c => c.Point, (Point)null);
        }

        [Fact]
        public void VerifyValid()
        {
            Fixture.CustomizePointM();

            var data = GeometryTranslator.Translate(Fixture.Create<PointM>());
            data.SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();

            Validator.ValidateAndThrow(data);
        }
    }
}
