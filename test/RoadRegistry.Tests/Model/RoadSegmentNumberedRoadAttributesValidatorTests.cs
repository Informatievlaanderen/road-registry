namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Messages;
    using Xunit;

    public class RoadSegmentNumberedRoadAttributesValidatorTests
    {
        public RoadSegmentNumberedRoadAttributesValidatorTests()
        {
            Fixture = new Fixture();
            Validator = new RoadSegmentNumberedRoadAttributesValidator();
        }

        public Fixture Fixture { get; }
        public RoadSegmentNumberedRoadAttributesValidator Validator { get; }

        [Fact]
        public void Ident8MustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(NumberedRoadNumber.All, candidate => candidate.ToString());
            var value = new Generator<string>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Ident8, value);
        }

        [Fact]
        public void DirectionMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(RoadSegmentNumberedRoadDirection.All, candidate => candidate.ToInt32());
            var value = new Generator<int>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Direction, (NumberedRoadSegmentDirection)value);
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
            var data = new RequestedRoadSegmentNumberedRoadAttributes
            {
                Ident8 = NumberedRoadNumber.All[new Random().Next(0, NumberedRoadNumber.All.Length)].ToString(),
                Direction = Fixture.Create<NumberedRoadSegmentDirection>(),
                Ordinal = new Generator<int>(Fixture).First(candidate => candidate >= 0)
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
