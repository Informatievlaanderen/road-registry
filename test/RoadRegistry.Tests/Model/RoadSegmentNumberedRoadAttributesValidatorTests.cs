namespace RoadRegistry.Model
{
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class RoadSegmentNumberedRoadAttributesValidatorTests
    {
        public RoadSegmentNumberedRoadAttributesValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizeAttributeId();
            Fixture.CustomizeNumberedRoadNumber();
            Fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
            Fixture.CustomizeRoadSegmentNumberedRoadDirection();
            Validator = new RoadSegmentNumberedRoadAttributesValidator();
        }

        public Fixture Fixture { get; }

        public RoadSegmentNumberedRoadAttributesValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void AttributeIdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.AttributeId, value);
        }

        [Fact]
        public void Ident8MustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Ident8, Fixture.Create<string>());
        }

        [Fact]
        public void DirectionMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Direction, Fixture.Create<string>());
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void OrdinalMustBeGreaterThanOrEqualToZero(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Ordinal, value);
        }

        [Fact]
        public void VerifyValid()
        {
            var data = new Messages.RoadSegmentNumberedRoadAttributes
            {
                AttributeId = Fixture.Create<AttributeId>(),
                Ident8 = Fixture.Create<NumberedRoadNumber>(),
                Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>(),
                Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
