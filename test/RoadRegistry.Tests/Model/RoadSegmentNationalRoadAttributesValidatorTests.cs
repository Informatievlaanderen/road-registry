namespace RoadRegistry.Model
{
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
            Fixture.CustomizeNationalRoadNumber();
            Validator = new RoadSegmentNationalRoadAttributesValidator();
        }

        public Fixture Fixture { get; }

        public RoadSegmentNationalRoadAttributesValidator Validator { get; }

        [Fact]
        public void Ident2MustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Ident2, Fixture.Create<string>());
        }

        [Fact]
        public void VerifyValid()
        {
            var data = new RequestedRoadSegmentNationalRoadAttributes
            {
                Ident2 = Fixture.Create<NationalRoadNumber>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
