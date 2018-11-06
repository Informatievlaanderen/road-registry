namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class RoadSegmentLanePropertiesValidatorTests
    {
        public RoadSegmentLanePropertiesValidatorTests()
        {
            Fixture = new Fixture();
            Validator = new RoadSegmentLanePropertiesValidator();
        }

        public Fixture Fixture { get; }
        public RoadSegmentLanePropertiesValidator Validator { get; }

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
            var data = new Commands.RoadSegmentLaneProperties
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
            var acceptable = Array.ConvertAll(RoadSegmentLaneDirection.All, candidate => candidate.ToInt32());
            var value = new Generator<int>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Direction, (Shared.LaneDirection)value);
        }

        [Fact]
        public void VerifyValid()
        {
            var positionGenerator = new Generator<double>(Fixture);
            var from = positionGenerator.First(candidate => candidate >= 0.0);

            var data = new Commands.RoadSegmentLaneProperties
            {
                FromPosition = from,
                ToPosition = positionGenerator.First(candidate => candidate > from),
                Count = new Generator<int>(Fixture).First(candidate => candidate >= 0 || candidate == -8 || candidate == -9),
                Direction = Fixture.Create<Shared.LaneDirection>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
