namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Messages;
    using Xunit;

    public class RoadSegmentHardeningPropertiesValidatorTests
    {
        public RoadSegmentHardeningPropertiesValidatorTests()
        {
            Fixture = new Fixture();
            Validator = new RoadSegmentHardeningPropertiesValidator();
        }

        public Fixture Fixture { get; }
        public RoadSegmentHardeningPropertiesValidator Validator { get; }

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
            var data = new RequestedRoadSegmentHardeningProperties
            {
                FromPosition = from,
                ToPosition = to
            };
            Validator.ShouldHaveValidationErrorFor(c => c.ToPosition, data);
        }

        [Fact]
        public void TypeMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(RoadSegmentHardeningType.All, candidate => candidate.ToInt32());
            var value = new Generator<int>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Type, (HardeningType)value);
        }

        [Fact]
        public void VerifyValid()
        {
            var positionGenerator = new Generator<double>(Fixture);
            var from = positionGenerator.First(candidate => candidate >= 0.0);

            var data = new RequestedRoadSegmentHardeningProperties
            {
                FromPosition = from,
                ToPosition = positionGenerator.First(candidate => candidate > from),
                Type = Fixture.Create<HardeningType>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}