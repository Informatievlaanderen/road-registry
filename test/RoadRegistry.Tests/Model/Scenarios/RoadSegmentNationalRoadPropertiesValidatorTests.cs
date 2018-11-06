namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class RoadSegmentNationalRoadPropertiesValidatorTests
    {
        public RoadSegmentNationalRoadPropertiesValidatorTests()
        {
            Fixture = new Fixture();
            Validator = new RoadSegmentNationalRoadPropertiesValidator();
        }

        public Fixture Fixture { get; }
        public RoadSegmentNationalRoadPropertiesValidator Validator { get; }

        [Fact]
        public void Ident2MustBeWithinDomain()
        {
            var acceptable = Array.ConvertAll(NationalRoadNumber.All, candidate => candidate.ToString());
            var value = new Generator<string>(Fixture).First(candidate => !acceptable.Contains(candidate));
            Validator.ShouldHaveValidationErrorFor(c => c.Ident2, value);
        }

        [Fact]
        public void VerifyValid()
        {
            var data = new Commands.RoadSegmentNationalRoadProperties
            {
                Ident2 = NationalRoadNumber.All[new Random().Next(0, NationalRoadNumber.All.Length)].ToString()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
