namespace RoadRegistry.Model
{
    using Aiv.Vbr.Shaperon;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class AddRoadNodeValidatorTests
    {
        public AddRoadNodeValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizeRoadNodeId();
            Fixture.CustomizeRoadNodeType();
            Validator = new AddRoadNodeValidator();
        }

        public Fixture Fixture { get; }

        public AddRoadNodeValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void IdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Id, value);
        }

        [Fact]
        public void TypeMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
        }

        [Fact]
        public void GeometryMustNotBeNull()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Geometry2, (Messages.RoadNodeGeometry)null);
        }

        [Fact]
        public void GeometryHasExpectedValidator()
        {
            Validator.ShouldHaveChildValidator(c => c.Geometry2, typeof(RoadNodeGeometryValidator));
        }

        [Fact]
        public void VerifyValid()
        {
            Fixture.CustomizePointM();

            var data = new Messages.AddRoadNode
            {
                Id = Fixture.Create<RoadNodeId>(),
                Type = Fixture.Create<RoadNodeType>(),
                Geometry2 = GeometryTranslator.Translate(Fixture.Create<PointM>())
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
