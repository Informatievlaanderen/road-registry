namespace RoadRegistry.Model
{
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class RoadSegmentNationalRoadAttributesValidatorTests
    {
        public RoadSegmentNationalRoadAttributesValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizeAttributeId();
            Fixture.CustomizeNationalRoadNumber();
            Validator = new RoadSegmentNationalRoadAttributesValidator();
        }

        public Fixture Fixture { get; }

        public RoadSegmentNationalRoadAttributesValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void AttributeIdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.AttributeId, value);
        }

        [Fact]
        public void Ident2MustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Ident2, Fixture.Create<string>());
        }

        [Fact]
        public void VerifyValid()
        {
            var data = new Messages.RoadSegmentNationalRoadAttributes
            {
                AttributeId = Fixture.Create<AttributeId>(),
                Ident2 = Fixture.Create<NationalRoadNumber>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
