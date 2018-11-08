namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Messages;
    using Xunit;

    public class RoadSegmentNationalRoadAttributesValidatorTests
    {
        public RoadSegmentNationalRoadAttributesValidatorTests()
        {
            Fixture = new Fixture();
            Validator = new RoadSegmentNationalRoadAttributesValidator();
        }

        public Fixture Fixture { get; }
        public RoadSegmentNationalRoadAttributesValidator Validator { get; }

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
            var data = new RequestedRoadSegmentNationalRoadAttributes
            {
                Ident2 = NationalRoadNumber.All[new Random().Next(0, NationalRoadNumber.All.Length)].ToString()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
