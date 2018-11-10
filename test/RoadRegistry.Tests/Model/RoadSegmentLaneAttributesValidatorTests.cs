namespace RoadRegistry.Model
{
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Messages;
    using Xunit;

    public class RoadSegmentLaneAttributesValidatorTests
    {
        public RoadSegmentLaneAttributesValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizeRoadSegmentPosition();
            Fixture.CustomizeRoadSegmentLaneCount();
            Fixture.CustomizeRoadSegmentLaneDirection();
            Validator = new RoadSegmentLaneAttributesValidator();
        }

        public Fixture Fixture { get; }
        public RoadSegmentLaneAttributesValidator Validator { get; }

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
            var data = new RequestedRoadSegmentLaneAttributes
            {
                FromPosition = from,
                ToPosition = to
            };
            Validator.ShouldHaveValidationErrorFor(c => c.ToPosition, data);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void CountMustBeGreaterThanOrEqualToZero(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Count, value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(-8)]
        [InlineData(-9)]
        public void CountCanBeGreaterThanOrEqualToZeroOrMinus8OrMinus9(int value)
        {
            Validator.ShouldNotHaveValidationErrorFor(c => c.Count, value);
        }

        [Fact]
        public void DirectionMustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Direction, Fixture.Create<string>());
        }

        [Fact]
        public void VerifyValid()
        {
            var positionGenerator = new Generator<decimal>(Fixture);
            var from = positionGenerator.First(candidate => candidate >= 0.0m);

            var data = new RequestedRoadSegmentLaneAttributes
            {
                FromPosition = from,
                ToPosition = positionGenerator.First(candidate => candidate > from),
                Count = Fixture.Create<RoadSegmentLaneCount>(),
                Direction = Fixture.Create<RoadSegmentLaneDirection>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
