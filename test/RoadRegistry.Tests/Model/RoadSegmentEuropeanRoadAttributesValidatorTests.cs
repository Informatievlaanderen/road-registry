namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class RoadSegmentEuropeanRoadAttributesValidatorTests
    {
        public RoadSegmentEuropeanRoadAttributesValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizeAttributeId();
            Validator = new RoadSegmentEuropeanRoadAttributesValidator();
        }

        public Fixture Fixture { get; }

        public RoadSegmentEuropeanRoadAttributesValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void AttributeIdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.AttributeId, value);
        }

        [Fact]
        public void RoadNumberMustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(EuropeanRoadNumber.All, candidate => candidate.ToString());
            var value = new Generator<string>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.RoadNumber, value);
        }

        [Fact]
        public void VerifyValid()
        {
            var data = new Messages.RoadSegmentEuropeanRoadAttributes
            {
                AttributeId = Fixture.Create<AttributeId>(),
                RoadNumber = EuropeanRoadNumber.All[new Random().Next(0, EuropeanRoadNumber.All.Length)].ToString()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
