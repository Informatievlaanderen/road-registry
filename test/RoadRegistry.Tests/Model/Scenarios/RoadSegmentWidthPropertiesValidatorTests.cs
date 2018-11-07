namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class RoadSegmentWidthPropertiesValidatorTests
    {
        public RoadSegmentWidthPropertiesValidatorTests()
        {
            Fixture = new Fixture();
            Validator = new RoadSegmentWidthPropertiesValidator();
        }

        public Fixture Fixture { get; }
        public RoadSegmentWidthPropertiesValidator Validator { get; }

        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(-0.1)]
        public void FromPositionMustBePositive(double value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.FromPosition, value);
        }

        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(0.1, 0.0)]
        [InlineData(0.1, 0.1)]
        public void ToPositionMustBeGreaterThanFromPosition(double from, double to)
        {
            var data = new Shared.RoadSegmentWidthProperties
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
            var positionGenerator = new Generator<double>(Fixture);
            var from = positionGenerator.First(candidate => candidate >= 0.0);

            var data = new Shared.RoadSegmentWidthProperties
            {
                FromPosition = from,
                ToPosition = positionGenerator.First(candidate => candidate > from),
                Width = new Generator<int>(Fixture).First(candidate => candidate >= 0 || candidate == -8 || candidate == -9)
            };

            Validator.ValidateAndThrow(data);
        }
    }
}