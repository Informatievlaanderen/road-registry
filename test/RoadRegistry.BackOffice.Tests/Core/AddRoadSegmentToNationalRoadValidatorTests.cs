namespace RoadRegistry.BackOffice.Core
{
    using AutoFixture;
    using FluentValidation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class AddRoadSegmentToNationalRoadValidatorTests
    {
        public AddRoadSegmentToNationalRoadValidatorTests()
        {
            Fixture = new Fixture();
            Fixture.CustomizeAttributeId();
            Fixture.CustomizeNationalRoadNumber();
            Validator = new AddRoadSegmentToNationalRoadValidator();
        }

        public Fixture Fixture { get; }

        public AddRoadSegmentToNationalRoadValidator Validator { get; }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        public void TemporaryAttributeIdMustBeGreaterThan(int value)
        {
            Validator.ShouldHaveValidationErrorFor(c => c.TemporaryAttributeId, value);
        }

        [Fact]
        public void Ident2MustBeWithinDomain()
        {
            Validator.ShouldHaveValidationErrorFor(c => c.Ident2, Fixture.Create<string>());
        }

        [Fact]
        public void VerifyValid()
        {
            var data = new Messages.AddRoadSegmentToNationalRoad
            {
                TemporaryAttributeId = Fixture.Create<AttributeId>(),
                Ident2 = Fixture.Create<NationalRoadNumber>()
            };

            Validator.ValidateAndThrow(data);
        }
    }
}
