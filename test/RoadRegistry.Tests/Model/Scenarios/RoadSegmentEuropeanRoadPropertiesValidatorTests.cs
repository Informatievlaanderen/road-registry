namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class RoadSegmentEuropeanRoadPropertiesValidatorTests
    {
        public RoadSegmentEuropeanRoadPropertiesValidatorTests()
        {
            Fixture = new Fixture();
            Validator = new RoadSegmentEuropeanRoadPropertiesValidator();
        }

        public Fixture Fixture { get; }
        public RoadSegmentEuropeanRoadPropertiesValidator Validator { get; }

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
            var data = new Commands.RoadSegmentEuropeanRoadProperties
            {
                RoadNumber = EuropeanRoadNumber.All[new Random().Next(0, EuropeanRoadNumber.All.Length)].ToString()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}