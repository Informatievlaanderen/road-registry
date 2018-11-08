namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
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
            var acceptable = Array.ConvertAll(RoadSegmentLaneDirection.All, candidate => candidate.ToInt32());
            var value = new Generator<int>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Direction, (LaneDirection)value);
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
                Count = new Generator<int>(Fixture).First(candidate => candidate >= 0 || candidate == -8 || candidate == -9),
                Direction = Fixture.Create<LaneDirection>()
            };

            Validator.ValidateAndThrow(data);
        }
    }

    public static class DynamicAttributePositionCases
    {
        public static IEnumerable<object[]> NegativeFromPosition
        {
            get
            {
                yield return new object[] {decimal.MinValue};
                yield return new object[] {-0.1m};
            }
        }

        public static IEnumerable<object[]> ToPositionLessThanFromPosition
        {
            get
            {
                yield return new object[] {0.0m, 0.0m};
                yield return new object[] {0.1m, 0.0m};
                yield return new object[] {0.1m, 0.1m};
            }
        }
    }
}
