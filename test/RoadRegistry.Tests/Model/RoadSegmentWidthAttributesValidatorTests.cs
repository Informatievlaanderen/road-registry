namespace RoadRegistry.Model
{
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Messages;
    using Xunit;

    public class RoadSegmentWidthAttributesValidatorTests
    {
        public RoadSegmentWidthAttributesValidatorTests()
        {
            Fixture = new Fixture();
            Validator = new RoadSegmentWidthPropertiesValidator();
        }

        public Fixture Fixture { get; }
        public RoadSegmentWidthPropertiesValidator Validator { get; }

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
            var data = new RequestedRoadSegmentWidthAttributes
            {
                FromPosition = from,
                ToPosition = to
            };
            Validator.ShouldHaveValidationErrorFor(c => c.ToPosition, data);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void WidthMustBeGreaterThanOrEqualToZero(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Width, value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(-8)]
        [InlineData(-9)]
        public void WidthCanBeGreaterThanOrEqualToZeroOrMinus8OrMinus9(int value)
        {
            Validator.ShouldNotHaveValidationErrorFor(c => c.Width, value);
        }

        [Fact]
        public void VerifyValid()
        {
            var positionGenerator = new Generator<decimal>(Fixture);
            var from = positionGenerator.First(candidate => candidate >= 0.0m);

            var data = new RequestedRoadSegmentWidthAttributes
            {
                FromPosition = from,
                ToPosition = positionGenerator.First(candidate => candidate > from),
                Width = new Generator<int>(Fixture).First(candidate => candidate >= 0 || candidate == -8 || candidate == -9)
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
