namespace RoadRegistry.Model
{
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Messages;
    using Xunit;

    public class RoadSegmentSurfaceAttributesValidatorTests
    {
        public RoadSegmentSurfaceAttributesValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizeRoadSegmentPosition();
            Fixture.CustomizeRoadSegmentSurfaceType();
            Validator = new RoadSegmentSurfaceAttributesValidator();
        }

        public Fixture Fixture { get; }

        public RoadSegmentSurfaceAttributesValidator Validator { get; }

        [Theory]
        [MemberData(nameof(DynamicAttributePositionCases.NegativeFromPosition), MemberType = typeof(DynamicAttributePositionCases))]
        public void FromPositionMustBePositive(decimal value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.FromPosition, value);
        }

        [Theory]
        [MemberData(nameof(DynamicAttributePositionCases.ToPositionLessThanFromPosition), MemberType = typeof(DynamicAttributePositionCases))]
        public void ToPositionMustBeGreaterThanFromPosition(decimal from, decimal to)
        {
            var data = new RequestedRoadSegmentSurfaceAttributes
            {
                FromPosition = from,
                ToPosition = to
            };
            Validator.ShouldHaveValidationErrorFor(c => c.ToPosition, data);
        }

        [Fact]
        public void TypeMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
        }

        [Fact]
        public void VerifyValid()
        {
            var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
            var from = positionGenerator.First(candidate => candidate >= 0.0m);

            var data = new RequestedRoadSegmentSurfaceAttributes
            {
                FromPosition = from,
                ToPosition = positionGenerator.First(candidate => candidate > from),
                Type = Fixture.Create<RoadSegmentSurfaceType>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
